using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagementSystem.Api.Infrastructure;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class ApiExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenValidationException_ReturnsBadRequest()
    {
        var handler = new ApiExceptionHandler(
            NullLogger<ApiExceptionHandler>.Instance,
            new TestHostEnvironment());

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/auth/login";
        context.Response.Body = new MemoryStream();

        var exception = new ValidationException(
        [
            new ValidationFailure("Code", "Code is required")
        ]);

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_WhenUnexpectedException_ReturnsInternalServerError()
    {
        var handler = new ApiExceptionHandler(
            NullLogger<ApiExceptionHandler>.Instance,
            new TestHostEnvironment { EnvironmentName = Environments.Development });

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/hr/leave/balances";
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException("boom"),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "TaskManagementSystem.Api";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
