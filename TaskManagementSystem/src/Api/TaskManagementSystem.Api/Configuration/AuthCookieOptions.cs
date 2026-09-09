namespace TaskManagementSystem.Api.Configuration;

public static class AuthCookieOptions
{
    public const string RefreshTokenName = "refreshToken";

    public static CookieOptions CreateRefreshTokenCookieOptions(DateTime expires, bool isHttps) =>
        new()
        {
            HttpOnly = true,
            Path = "/",
            Expires = expires,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax
        };

    public static CookieOptions CreateDeleteRefreshTokenCookieOptions(bool isHttps) =>
        new()
        {
            HttpOnly = true,
            Path = "/",
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax
        };
}
