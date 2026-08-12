using System.Net;

namespace AutomatedTaskSystem.Helper;

public static class ClientIpHelper
{
    /// <summary>
    /// Resolves the real client IP when the app sits behind IIS / a reverse proxy.
    /// Falls back to connection RemoteIpAddress (often ::1 or 127.0.0.1 in local/proxy setups).
    /// </summary>
    public static string? GetClientIp(HttpContext httpContext)
    {
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            // First entry is the original client; later entries are proxies
            var first = forwardedFor.Split(',')[0].Trim();
            if (TryNormalizeIp(first, out var forwardedIp))
                return forwardedIp;
        }

        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(realIp) && TryNormalizeIp(realIp.Trim(), out var real))
            return real;

        var remote = httpContext.Connection.RemoteIpAddress;
        if (remote is null)
            return null;

        if (remote.IsIPv4MappedToIPv6)
            remote = remote.MapToIPv4();

        return remote.ToString();
    }

    private static bool TryNormalizeIp(string value, out string ip)
    {
        ip = value;
        // Strip port if present: "1.2.3.4:1234" or "[::1]:1234"
        if (value.StartsWith('[') && value.Contains(']'))
        {
            var end = value.IndexOf(']');
            value = value[1..end];
        }
        else if (value.Count(c => c == ':') == 1)
        {
            value = value.Split(':')[0];
        }

        if (!IPAddress.TryParse(value, out var parsed))
            return false;

        if (parsed.IsIPv4MappedToIPv6)
            parsed = parsed.MapToIPv4();

        ip = parsed.ToString();
        return true;
    }
}
