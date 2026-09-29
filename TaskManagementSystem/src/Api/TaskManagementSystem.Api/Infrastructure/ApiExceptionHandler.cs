using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using TaskManagementSystem.BuildingBlocks.Application.Behaviors;

namespace TaskManagementSystem.Api.Infrastructure;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            logger.LogDebug(
                "Request canceled for {Method} {Path}. TraceId={TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                Activity.Current?.TraceId.ToString());

            httpContext.Response.StatusCode = 499;
            return true;
        }

        if (exception is ValidationException validationException)
        {
            var errors = ValidationErrorGrouping.GroupByProperty(validationException.Errors);

            logger.LogWarning(
                "Validation failed for {Method} {Path}. TraceId={TraceId} ValidationCode={ValidationCode} Errors={ValidationErrors}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                Activity.Current?.TraceId.ToString(),
                "validation_failed",
                errors);

            await WriteOkFailureAsync(
                httpContext,
                validationException.Message,
                "validation_failed",
                errors,
                cancellationToken);

            return true;
        }

        logger.LogError(
            exception,
            "Unhandled exception processing {Method} {Path}. TraceId={TraceId} StatusCode={StatusCode} ExceptionType={ExceptionType}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            Activity.Current?.TraceId.ToString(),
            StatusCodes.Status500InternalServerError,
            exception.GetType().Name);

        var detail = environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred.";

        await WriteOkFailureAsync(
            httpContext,
            detail,
            "unexpected_error",
            null,
            cancellationToken);

        return true;
    }

    private static async Task WriteOkFailureAsync(
        HttpContext httpContext,
        string message,
        string? code,
        IReadOnlyDictionary<string, string[]>? errors,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        await httpContext.Response.WriteAsJsonAsync(
            ApiFailureBody.Create(message, code, errors),
            cancellationToken);
    }
}
