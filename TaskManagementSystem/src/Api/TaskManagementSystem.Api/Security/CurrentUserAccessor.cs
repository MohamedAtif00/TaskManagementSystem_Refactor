namespace TaskManagementSystem.Api.Security;

public interface ICurrentUserAccessor
{
    int? GetUserId();

    int GetRequiredUserId();
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
}
