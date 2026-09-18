using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Auth;

namespace TaskManagementSystem.Api.Endpoints.Auth;

internal static class AuthCookieHelpers
{
    internal static async Task<string?> ResolveRefreshTokenAsync(HttpContext httpContext)
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

    internal static void SetRefreshTokenCookie(HttpContext httpContext, string token, DateTime expires)
    {
        httpContext.Response.Cookies.Append(
            AuthCookieOptions.RefreshTokenName,
            token,
            AuthCookieOptions.CreateRefreshTokenCookieOptions(expires, httpContext.Request.IsHttps));
    }
}
