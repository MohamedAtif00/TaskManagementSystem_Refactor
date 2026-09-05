using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Audit;

public sealed class EfAuditStore(AuditDbContext context) : IAuditStore
{
    public async Task AppendAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        context.AuditLog.Add(new AuditLogEntry
        {
            CorrelationId = entry.CorrelationId,
            UserId = entry.UserId,
            ActionName = entry.ActionName,
            OccurredOnUtc = entry.OccurredOnUtc,
            Success = entry.Success
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
