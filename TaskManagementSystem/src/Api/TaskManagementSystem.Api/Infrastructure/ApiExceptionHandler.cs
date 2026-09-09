using FluentValidation;
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
            await WriteProblemDetailsAsync(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Validation failed",
                validationException.Message,
                new Dictionary<string, object?>
                {
                    ["code"] = "validation_failed",
                    ["errors"] = validationException.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(error => error.ErrorMessage).ToArray())
                },
                cancellationToken);

            return true;
        }

        logger.LogError(exception, "Unhandled exception processing {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

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
