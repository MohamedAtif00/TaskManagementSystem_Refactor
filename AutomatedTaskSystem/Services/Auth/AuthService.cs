using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Hub;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using AutomatedTaskSystem.Services.SessionTracking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly DataContext _context;
    private readonly IUserSessionService _sessionService;
    private readonly IHubContext<UserHub> _hubContext;

    public AuthService(
        ITokenService tokenService,
        DataContext context,
        IUserSessionService sessionService,
        IHubContext<UserHub> hubContext)
    {
        _tokenService = tokenService;
        _context = context;
        _sessionService = sessionService;
        _hubContext = hubContext;
    }

    public async Task<ActionResult<ResponseService<Responses.AuthInfoDTO>>> AboutUser()
    {
        var tokenRes = _tokenService.GetUserIdFromToken();

        if (tokenRes.Error)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = tokenRes.Message }
            );

        var convertable = Int32.TryParse(tokenRes.Data, out int uid);

        var user = await _context.Users
            .Where(u => u.Id == uid && !u.Archived)
            .Include(u => u.Group)
            .FirstOrDefaultAsync();

        var notification = await _context.Notifications.Where(n => n.UserId == user.Id && !n.IsRead).CountAsync();

        if (user is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid token" }
            );

        return new ResponseService<Responses.AuthInfoDTO>
        {
            Data = new Responses.AuthInfoDTO
            {
                Id = user.Id,
                Group = user.Group?.Name ?? "",
                Role = user.Role,
                Name = user.Name,
                Notifications = notification
            },
            Error = false,
            Message = "User info"
        };
    }

    public async Task<ActionResult<ResponseService<string>>> Login(
        string Code,
        HttpRequest request,
        HttpResponse response
    )
    {
        var user = await _context.Users
            .Where(u => u.Code == Code && !u.Archived)
            .FirstOrDefaultAsync();

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid code" }
            );

        var accessToken = _tokenService.CreateToken(user);
        var refreshToken = await generateRefreshToken(user);

        setTokenInCookie(refreshToken, response);

        var ip = ClientIpHelper.GetClientIp(request.HttpContext);
        var ua = request.Headers.UserAgent.ToString();
        await _sessionService.StartSessionAsync(user.Id, refreshToken.Token, ip, ua);

        return new ResponseService<string>
        {
            Data = accessToken,
            Error = false,
            Message = "User authenticated"
        };
    }

    public async Task<ActionResult<BaseResponseService>> Logout(
        HttpRequest request,
        HttpResponse response
    )
    {
        var refreshToken = request.Cookies["refreshToken"];

        if (refreshToken is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid refesh token" }
            );

        var token = await _context.RefreshTokens
            .Where(t => t.Token == refreshToken)
            .FirstOrDefaultAsync();

        if (token is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid refesh token" }
            );

        var now = DateTime.UtcNow;
        var tokenExpired = AsUtc(token.Expires) <= now;
        var reason = tokenExpired
            ? SessionLogoutReason.TokenExpired
            : SessionLogoutReason.Manual;

        token.Used = true;
        await _context.SaveChangesAsync();

        await _sessionService.EndSessionByRefreshTokenAsync(refreshToken, reason);

        clearRefreshTokenCookie(response);

        return new BaseResponseService
        {
            Error = false,
            Message = reason == SessionLogoutReason.TokenExpired
                ? "User logout (session expired)"
                : "User logout"
        };
    }

    public async Task<ActionResult<BaseResponseService>> EndSessionExpired(
        HttpRequest request,
        HttpResponse response)
    {
        var refreshToken = request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var token = await _context.RefreshTokens
                .Where(t => t.Token == refreshToken)
                .FirstOrDefaultAsync();

            if (token is not null)
            {
                token.Used = true;
                await _context.SaveChangesAsync();
            }

            await _sessionService.EndSessionByRefreshTokenAsync(
                refreshToken,
                SessionLogoutReason.TokenExpired);
        }

        clearRefreshTokenCookie(response);

        return new BaseResponseService
        {
            Error = false,
            Message = "Session expired — logged out by system"
        };
    }

    public async Task<ActionResult<BaseResponseService>> ForceLogout(int userId)
    {
        var actor = await GetAuthedUser();
        if (actor is null || actor.Role != UserRoleEnum.Owner)
        {
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Owner access required" }
            );
        }

        var target = await _context.Users
            .Where(u => u.Id == userId && !u.Archived)
            .FirstOrDefaultAsync();

        if (target is null)
        {
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "User not found" }
            );
        }

        await _sessionService.InvalidateRefreshTokensForUserAsync(userId);
        await _sessionService.EndOpenSessionsForUserAsync(userId, SessionLogoutReason.ForcedByAdmin);

        await _hubContext.Clients.User(userId.ToString()).SendAsync("ForceLogout", new
        {
            reason = nameof(SessionLogoutReason.ForcedByAdmin)
        });

        return new BaseResponseService { Error = false, Message = "User force-logged out" };
    }

    public async Task<ActionResult<ResponseService<string>>> RefreshToken(
        HttpRequest request,
        HttpResponse response
    )
    {
        var refreshToken = request.Cookies["refreshToken"];

        if (refreshToken is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "No refresh token" }
            );

        var foundToken = await _context.RefreshTokens
            .Where(t => t.Token == refreshToken)
            .Include(t => t.User)
            .FirstOrDefaultAsync();

        if (foundToken is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid Refresh Token" }
            );

        if (foundToken.Used || DateTime.UtcNow > AsUtc(foundToken.Expires))
        {
            await _sessionService.EndSessionByRefreshTokenAsync(
                refreshToken,
                SessionLogoutReason.TokenExpired);
            clearRefreshTokenCookie(response);
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Token expired" }
            );
        }

        foundToken.Used = true;
        var newToken = _tokenService.GenerateRefreshToken(foundToken.User);
        _context.RefreshTokens.Add(newToken);
        await _context.SaveChangesAsync();

        // Keep the open UserSession linked to the active refresh token
        var openSession = await _context.UserSessions
            .Where(s => s.RefreshToken == refreshToken && s.LogoutAt == null)
            .FirstOrDefaultAsync();
        if (openSession is not null)
            openSession.RefreshToken = newToken.Token;

        await _context.SaveChangesAsync();

        setTokenInCookie(newToken, response);

        return new ResponseService<string>
        {
            Data = _tokenService.CreateToken(foundToken.User),
            Error = false,
            Message = "Token refreshed"
        };
    }

    private void setTokenInCookie(RefreshToken token, HttpResponse response)
    {
        var opts = new CookieOptions { HttpOnly = true, Expires = token.Expires };
        response.Cookies.Append("refreshToken", token.Token, opts);
    }

    private static void clearRefreshTokenCookie(HttpResponse response)
    {
        response.Cookies.Delete("refreshToken");
        response.Cookies.Append("refreshToken", "", new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });
    }

    private static DateTime AsUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private async Task<RefreshToken> generateRefreshToken(User user)
    {
        var rt = _tokenService.GenerateRefreshToken(user);

        _context.RefreshTokens.Add(rt);
        await _context.SaveChangesAsync();

        return rt;
    }

    public async Task<User?> GetAuthedUser()
    {
        var authRes = _tokenService.GetUserIdFromToken();
        if (authRes.Error)
            return null;

        var parseStatus = Int32.TryParse(authRes.Data, out int userId);
        if (!parseStatus)
            return null;

        return await _context.Users
            .Where(u => !u.Archived && u.Id == userId)
            .FirstOrDefaultAsync();
    }
}
