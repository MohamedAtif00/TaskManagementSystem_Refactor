using System.Security.Claims;

namespace TaskManagementSystem.Api.Security;

public interface ICurrentUserAccessor
{
    int? GetUserId();

    int GetRequiredUserId();

    string? GetRole();

    string GetRequiredRole();

    int? GetTeamId();
}

internal sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public int? GetUserId()
    {
        var idValue = httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;
        return int.TryParse(idValue, out var userId) ? userId : null;
    }

    public int GetRequiredUserId() =>
        GetUserId() ?? throw new UnauthorizedAccessException("User is not authenticated.");

    public string? GetRole() =>
        httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

    public string GetRequiredRole() =>
        GetRole() ?? throw new UnauthorizedAccessException("User role is not available.");

    public int? GetTeamId()
    {
        var teamIdValue = httpContextAccessor.HttpContext?.User.FindFirst("TeamId")?.Value;
        return int.TryParse(teamIdValue, out var teamId) ? teamId : null;
    }
}
