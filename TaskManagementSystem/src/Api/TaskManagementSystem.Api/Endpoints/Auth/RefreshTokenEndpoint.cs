using MediatR;
using TaskManagementSystem.Api.Contracts.Auth;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Features.RefreshToken;

namespace TaskManagementSystem.Api.Endpoints.Auth;

/// <summary>
/// POST /auth/refresh-token — Rotates session using refresh token from cookie or JSON body.
/// Sets new refresh-token cookie and returns new JWT access token.
/// </summary>
public static class RefreshTokenEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPost("/refresh-token", HandleAsync);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var refreshToken = await AuthCookieHelpers.ResolveRefreshTokenAsync(httpContext);
        var result = await mediator.Send(new RefreshSessionCommand(refreshToken ?? string.Empty), cancellationToken);

        return result.ToHttpResult(refreshResult =>
        {
            AuthCookieHelpers.SetRefreshTokenCookie(
                httpContext,
                refreshResult.RefreshToken.Token,
                refreshResult.RefreshToken.Expires);

            return Results.Ok(new AccessTokenResponse(refreshResult.AccessToken));
        });
    }
}
