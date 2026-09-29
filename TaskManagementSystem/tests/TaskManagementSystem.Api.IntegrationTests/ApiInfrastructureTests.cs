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
    public async Task ToProblemResult_WhenInvalidLoginCode_ReturnsOkFailureBody()
    {
        var context = CreateHttpContext();
        var error = new ResultError("invalid_login_code", "Invalid code");

        var httpResult = ResultHttpMapper.ToProblemResult(error);
        await httpResult.ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

        var failure = await ReadFailureJsonAsync(context);
        failure.GetProperty("success").GetBoolean().Should().BeFalse();
        failure.GetProperty("code").GetString().Should().Be("invalid_login_code");
        failure.GetProperty("message").GetString().Should().Be("Invalid code");
    }

    [Fact]
    public async Task ToProblemResult_WhenOtherError_ReturnsOkFailureBody()
    {
        var context = CreateHttpContext();
        var error = new ResultError("invalid_refresh_token", "Token expired");

        var httpResult = ResultHttpMapper.ToProblemResult(error);
        await httpResult.ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

        var failure = await ReadFailureJsonAsync(context);
        failure.GetProperty("success").GetBoolean().Should().BeFalse();
        failure.GetProperty("code").GetString().Should().Be("invalid_refresh_token");
    }

    [Fact]
    public async Task ToProblemResult_WhenForbidden_Returns403ProblemDetails()
    {
        var context = CreateHttpContext();
        var error = new ResultError("leave_opinion_not_authorized", "Not allowed");

        var httpResult = ResultHttpMapper.ToProblemResult(error);
        await httpResult.ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var problem = await ReadFailureJsonAsync(context);
        problem.GetProperty("code").GetString().Should().Be("leave_opinion_not_authorized");
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

    private static async Task<JsonElement> ReadFailureJsonAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
    }
}
