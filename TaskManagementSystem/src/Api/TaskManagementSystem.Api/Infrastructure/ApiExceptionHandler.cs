using System.Diagnostics;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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
        if (exception is ValidationException validationException)
        {
            var errors = GroupValidationErrors(validationException.Errors);

            logger.LogWarning(
                "Validation failed for {Method} {Path}. TraceId={TraceId} ValidationCode={ValidationCode} Errors={ValidationErrors}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                Activity.Current?.TraceId.ToString(),
                "validation_failed",
                errors);

            await WriteProblemDetailsAsync(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Validation failed",
                validationException.Message,
                new Dictionary<string, object?>
                {
                    ["code"] = "validation_failed",
                    ["errors"] = errors
                },
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

        await WriteProblemDetailsAsync(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            detail,
            new Dictionary<string, object?> { ["code"] = "unexpected_error" },
            cancellationToken);

        return true;
    }

    private static Dictionary<string, string[]> GroupValidationErrors(IEnumerable<ValidationFailure> failures) =>
        failures
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

    private static async Task WriteProblemDetailsAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        IDictionary<string, object?> extensions,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        foreach (var (key, value) in extensions)
        {
            problemDetails.Extensions[key] = value;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    }
}
