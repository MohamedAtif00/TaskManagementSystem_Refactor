using Microsoft.AspNetCore.Diagnostics;
using TaskManagementSystem.Api.Contracts.Legacy;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Api.Infrastructure;

public sealed class LegacyExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            InvalidLoginCodeException => (StatusCodes.Status404NotFound, "Invalid code"),
            InvalidRefreshTokenException invalidRefreshToken =>
                (StatusCodes.Status400BadRequest, invalidRefreshToken.Message),
            UserNotFoundException => (StatusCodes.Status400BadRequest, "Invalid token"),
            _ => (0, null)
        };

        if (statusCode == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            new BaseResponseService { Error = true, Message = message! },
            cancellationToken);

        return true;
    }
}
