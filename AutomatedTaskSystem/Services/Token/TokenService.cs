using Microsoft.IdentityModel.Tokens;
using AutomatedTaskSystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.TokenService;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
    {
        _config = config;
        _key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config.GetSection("AppSetting:Token").Value)
        );
        _httpContextAccessor = httpContextAccessor;
    }

    public string CreateToken(User user)
    {
        var claims = new List<Claim>()
        {
            new Claim("Id", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(User user)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.Now.AddDays(10),
            Created = DateTime.Now,
            User = user,
            UserId = user.Id
        };
        return refreshToken;
    }

    public ResponseService<string> GetUserIdFromToken()
    {
        if (_httpContextAccessor.HttpContext is not null)
        {
            var data = _httpContextAccessor.HttpContext.User.FindFirstValue("Id");
            if (data is null)
                return new ResponseService<string> { Error = true, Message = "Invalid Request." };
            return new ResponseService<string>
            {
                Error = false,
                Message = "User id",
                Data = data
            };
        }

        return new ResponseService<string> { Error = true, Message = "Invalid Request." };
    }
}
