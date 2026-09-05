using System.Diagnostics;
using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Api.Infrastructure;

internal sealed class AuditContext(IHttpContextAccessor httpContextAccessor) : IAuditContext
{
    public int? UserId
    {
        get
        {
            var idValue = httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;
            return int.TryParse(idValue, out var userId) ? userId : null;
        }
    }

    public string CorrelationId =>
        Activity.Current?.TraceId.ToString()
        ?? httpContextAccessor.HttpContext?.TraceIdentifier
        ?? Guid.NewGuid().ToString("N");
}
