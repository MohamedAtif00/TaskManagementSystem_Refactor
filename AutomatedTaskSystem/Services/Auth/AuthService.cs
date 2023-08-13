using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly DataContext _context;

    public AuthService(ITokenService tokenService, DataContext context)
    {
        _tokenService = tokenService;
        _context = context;
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

        if (user is null)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid token" }
            );

        return new ResponseService<Responses.AuthInfoDTO>
        {
            Data = new Responses.AuthInfoDTO
            {
                Id = user.Id,
                Group = user.Group.Name,
                Role = user.RoleId,
                Name = user.Name
            },
            Error = false,
            Message = "User info"
        };
    }

    public async Task<ActionResult<ResponseService<string>>> Login(
        string Code,
        HttpResponse response
    )
    {
        var user = await _context.Users
            .Where(u => u.Code.ToLower() == Code.ToLower() && !u.Archived)
            .FirstOrDefaultAsync();

        if (user is null)
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "Invalid code" }
            );

        var accessToken = _tokenService.CreateToken(user);
        var refreshToken = await generateRefreshToken(user);

        setTokenInCookie(refreshToken, response);

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

        if (token.Used && token.Expires < DateTime.Now)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Token expired" }
            );

        token.Used = true;

        await _context.SaveChangesAsync();

        return new BaseResponseService { Error = false, Message = "User logout" };
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

        if (foundToken.Used && DateTime.Now > foundToken.Expires)
            return new BadRequestObjectResult(
                new BaseResponseService { Error = true, Message = "Token expired" }
            );

        foundToken.Used = false;

        var newToken = _tokenService.GenerateRefreshToken(foundToken.User);

        setTokenInCookie(newToken, response);

        return new ResponseService<string>
        {
            Data = _tokenService.CreateToken(foundToken.User),
            Error = false,
            Message = "Token refreshed"
        };
    }

    private void setTokenInCookie(RefreshToken token, HttpResponse request)
    {
        var opts = new CookieOptions { HttpOnly = true, Expires = token.Expires };
        request.Cookies.Append("refreshToken", token.Token, opts);
    }

    private async Task<RefreshToken> generateRefreshToken(User user)
    {
        var rt = _tokenService.GenerateRefreshToken(user);

        _context.RefreshTokens.Add(rt);
        await _context.SaveChangesAsync();

        return rt;
    }
}
