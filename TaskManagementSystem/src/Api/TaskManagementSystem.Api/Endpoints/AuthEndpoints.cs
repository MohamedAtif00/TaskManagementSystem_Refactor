using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Auth;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Features.AboutMe;
using TaskManagementSystem.Modules.Identity.Features.Authenticate;
using TaskManagementSystem.Modules.Identity.Features.Logout;
using TaskManagementSystem.Modules.Identity.Features.RefreshToken;

namespace TaskManagementSystem.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", LoginAsync);
        group.MapPost("/refresh-token", RefreshTokenAsync);
        group.MapPost("/logout", LogoutAsync);
        group.MapPost("/about-me", AboutMeAsync).RequireAuthorization();

        return group;
    }

    private static async Task<IResult> AboutMeAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new AboutMeQuery(userId), cancellationToken);

        return result.ToHttpResult(profile => Results.Ok(new AuthInfoResponse
        {
            Id = profile.Id,
            Name = profile.Name,
            Role = profile.Role,
            RoleName = profile.RoleName,
            Permissions = profile.Permissions.ToArray(),
            Group = profile.Group,
            Notifications = profile.Notifications
        }));
    }

    private static async Task<IResult> LogoutAsync(
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var refreshToken = await ResolveRefreshTokenAsync(httpContext);
        var result = await mediator.Send(new LogoutCommand(refreshToken), cancellationToken);

        return result.ToHttpResult(_ =>
        {
            httpContext.Response.Cookies.Delete(
                AuthCookieOptions.RefreshTokenName,
                AuthCookieOptions.CreateDeleteRefreshTokenCookieOptions(httpContext.Request.IsHttps));

            return Results.NoContent();
        });
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AuthenticateCommand(request.Code), cancellationToken);

        return result.ToHttpResult(authenticateResult =>
        {
            SetRefreshTokenCookie(
                httpContext,
                authenticateResult.RefreshToken.Token,
                authenticateResult.RefreshToken.Expires);

            return Results.Ok(new AccessTokenResponse(authenticateResult.AccessToken));
        });
    }

    private static async Task<IResult> RefreshTokenAsync(
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var refreshToken = await ResolveRefreshTokenAsync(httpContext);
        var result = await mediator.Send(new RefreshSessionCommand(refreshToken ?? string.Empty), cancellationToken);

        return result.ToHttpResult(refreshResult =>
        {
            SetRefreshTokenCookie(
                httpContext,
                refreshResult.RefreshToken.Token,
                refreshResult.RefreshToken.Expires);

            return Results.Ok(new AccessTokenResponse(refreshResult.AccessToken));
        });
    }

    private static async Task<string?> ResolveRefreshTokenAsync(HttpContext httpContext)
    {
        var cookieToken = httpContext.Request.Cookies[AuthCookieOptions.RefreshTokenName];
        if (!string.IsNullOrWhiteSpace(cookieToken))
        {
            return cookieToken;
        }

        if (!httpContext.Request.HasJsonContentType())
        {
            return null;
        }

        var body = await httpContext.Request.ReadFromJsonAsync<RefreshTokenRequest>(httpContext.RequestAborted);
        return string.IsNullOrWhiteSpace(body?.RefreshToken) ? null : body.RefreshToken;
    }

    private static void SetRefreshTokenCookie(HttpContext httpContext, string token, DateTime expires)
    {
        httpContext.Response.Cookies.Append(
            AuthCookieOptions.RefreshTokenName,
            token,
            AuthCookieOptions.CreateRefreshTokenCookieOptions(expires, httpContext.Request.IsHttps));
    }
}
