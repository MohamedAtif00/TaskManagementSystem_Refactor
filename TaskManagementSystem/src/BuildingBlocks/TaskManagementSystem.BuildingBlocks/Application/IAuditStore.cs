namespace TaskManagementSystem.BuildingBlocks.Application;

public sealed record AuditEntry(
    string CorrelationId,
    int? UserId,
    string ActionName,
    DateTime OccurredOnUtc,
    bool Success);

public interface IAuditStore
{
    Task AppendAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
