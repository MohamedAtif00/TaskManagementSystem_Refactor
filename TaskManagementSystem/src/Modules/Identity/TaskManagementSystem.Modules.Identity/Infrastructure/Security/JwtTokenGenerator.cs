using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Security;

internal sealed class JwtTokenGenerator(IConfiguration configuration, TimeProvider timeProvider) : ITokenGenerator
{
    private const int AccessTokenDays = 2;
    private const int RefreshTokenDays = 10;

    public string CreateAccessToken(User user)
    {
        var signingKey = GetSigningKey();
        var claims = new List<Claim>
        {
            new("Id", user.Id.ToString()),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature);
        var expires = timeProvider.GetUtcNow().UtcDateTime.AddDays(AccessTokenDays);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken CreateRefreshToken(User user, DateTime utcNow) =>
        RefreshToken.Create(
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            utcNow,
            utcNow.AddDays(RefreshTokenDays),
            user.Id);

    private SymmetricSecurityKey GetSigningKey()
    {
        var secret = configuration.GetSection("AppSetting:Token").Value
            ?? throw new InvalidOperationException("AppSetting:Token is not configured.");

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }
}
