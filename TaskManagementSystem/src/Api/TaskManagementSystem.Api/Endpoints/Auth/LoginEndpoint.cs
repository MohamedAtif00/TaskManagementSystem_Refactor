using MediatR;
using TaskManagementSystem.Api.Contracts.Auth;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Features.Authenticate;

namespace TaskManagementSystem.Api.Endpoints.Auth;

/// <summary>
/// POST /auth/login — Authenticates by HR code. No auth required.
/// Sets refresh-token cookie and returns JWT access token in body.
/// </summary>
public static class LoginEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPost("/login", HandleAsync);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        LoginRequest request,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AuthenticateCommand(request.Code), cancellationToken);

        return result.ToHttpResult(authenticateResult =>
        {
            AuthCookieHelpers.SetRefreshTokenCookie(
                httpContext,
                authenticateResult.RefreshToken.Token,
                authenticateResult.RefreshToken.Expires);

            return Results.Ok(new AccessTokenResponse(authenticateResult.AccessToken));
        });
    }
}
