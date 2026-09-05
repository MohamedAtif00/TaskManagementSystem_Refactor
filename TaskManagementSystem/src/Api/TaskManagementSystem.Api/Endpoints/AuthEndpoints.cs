using MediatR;
using TaskManagementSystem.Api.Contracts.Legacy;
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
        return Results.Ok(new ResponseService<AuthResponses.AuthInfoDto>
        {
            Data = new AuthResponses.AuthInfoDto
            {
                Id = result.Id,
                Name = result.Name,
                Role = result.Role,
                Group = result.Group,
                Notifications = result.Notifications
            },
            Error = false,
            Message = "User info"
        });
    }

    private static async Task<IResult> LogoutAsync(
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new LogoutCommand(httpContext.Request.Cookies["refreshToken"]), cancellationToken);
        httpContext.Response.Cookies.Delete("refreshToken");
        return Results.Ok(new BaseResponseService { Error = false, Message = "User logout" });
    }

    private static async Task<IResult> LoginAsync(
        AuthRequests.LoginDto request,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AuthenticateCommand(request.Code), cancellationToken);
        SetRefreshTokenCookie(httpContext.Response, result.RefreshToken.Token, result.RefreshToken.Expires);
        return Results.Ok(new ResponseService<string>
        {
            Data = result.AccessToken,
            Error = false,
            Message = "User authenticated"
        });
    }

    private static async Task<IResult> RefreshTokenAsync(
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var refreshToken = httpContext.Request.Cookies["refreshToken"];
        var result = await mediator.Send(new RefreshSessionCommand(refreshToken ?? string.Empty), cancellationToken);
        SetRefreshTokenCookie(httpContext.Response, result.RefreshToken.Token, result.RefreshToken.Expires);
        return Results.Ok(new ResponseService<string>
        {
            Data = result.AccessToken,
            Error = false,
            Message = "Token refreshed"
        });
    }

    private static void SetRefreshTokenCookie(HttpResponse response, string token, DateTime expires)
    {
        response.Cookies.Append(
            "refreshToken",
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Expires = expires
            });
    }
}
