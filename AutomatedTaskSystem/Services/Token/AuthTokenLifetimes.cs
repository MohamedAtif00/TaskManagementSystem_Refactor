namespace AutomatedTaskSystem.Services.TokenService;

/// <summary>
/// Access JWT is short-lived and renewed silently via refresh token while the user is active.
/// Refresh token is a sliding window renewed on each successful refresh.
/// </summary>
public static class AuthTokenLifetimes
{
    public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromMinutes(15);
}
