namespace TaskManagementSystem.BuildingBlocks.Persistence.Audit;

public sealed class AuditLogEntry
{
    public long Id { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public int? UserId { get; set; }

    public string ActionName { get; set; } = string.Empty;

    public DateTime OccurredOnUtc { get; set; }

    public bool Success { get; set; }
}
