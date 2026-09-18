namespace TaskManagementSystem.BuildingBlocks.Application.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public DateTime OccurredOnUtc { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTime? ProcessedOnUtc { get; set; }

    public string? Error { get; set; }

    public int Attempts { get; set; }
}
