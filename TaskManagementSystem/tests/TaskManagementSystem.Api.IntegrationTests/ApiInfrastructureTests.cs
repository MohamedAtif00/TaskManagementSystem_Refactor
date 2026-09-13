using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.BuildingBlocks.Domain;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class ResultHttpMapperTests
{
    [Fact]
    public async Task ToHttpResult_WhenSuccess_ExecutesSuccessDelegate()
    {
        var context = CreateHttpContext();
        var result = Result.Ok("token");

        var httpResult = result.ToHttpResult(value => Results.Ok(new { accessToken = value }));
        await httpResult.ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task ToProblemResult_WhenInvalidLoginCode_Returns404WithCodeExtension()
    {
        var context = CreateHttpContext();
        var error = new ResultError("invalid_login_code", "Invalid code");

        var httpResult = ResultHttpMapper.ToProblemResult(error);
        await httpResult.ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var problem = await ReadProblemJsonAsync(context);
        problem.GetProperty("code").GetString().Should().Be("invalid_login_code");
        problem.GetProperty("detail").GetString().Should().Be("Invalid code");
    }

    [Fact]
    public async Task ToProblemResult_WhenOtherError_Returns400WithCodeExtension()
    {
        var context = CreateHttpContext();
        var error = new ResultError("invalid_refresh_token", "Token expired");

        var httpResult = ResultHttpMapper.ToProblemResult(error);
        await httpResult.ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var problem = await ReadProblemJsonAsync(context);
        problem.GetProperty("code").GetString().Should().Be("invalid_refresh_token");
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() },
            RequestServices = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
        };

        return context;
    }

    private static async Task<JsonElement> ReadProblemJsonAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
    }
}
