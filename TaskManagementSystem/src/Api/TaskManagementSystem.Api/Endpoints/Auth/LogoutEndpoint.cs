using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Features.Logout;

namespace TaskManagementSystem.Api.Endpoints.Auth;

/// <summary>
/// POST /auth/logout — Invalidates refresh session and deletes refresh-token cookie. No auth required.
/// </summary>
public static class LogoutEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPost("/logout", HandleAsync);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var refreshToken = await AuthCookieHelpers.ResolveRefreshTokenAsync(httpContext);
        var result = await mediator.Send(new LogoutCommand(refreshToken), cancellationToken);

        return result.ToHttpResult(_ =>
        {
            httpContext.Response.Cookies.Delete(
                AuthCookieOptions.RefreshTokenName,
                AuthCookieOptions.CreateDeleteRefreshTokenCookieOptions(httpContext.Request.IsHttps));

            return Results.NoContent();
        });
    }
}
