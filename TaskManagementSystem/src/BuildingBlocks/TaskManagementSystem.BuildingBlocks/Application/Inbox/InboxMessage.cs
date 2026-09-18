namespace TaskManagementSystem.BuildingBlocks.Application.Inbox;

public sealed class InboxMessage
{
    public Guid Id { get; set; }

    public Guid IntegrationEventId { get; set; }

    public string ConsumerName { get; set; } = string.Empty;

    public DateTime OccurredOnUtc { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTime? ProcessedOnUtc { get; set; }

    public string? Error { get; set; }

    public int Attempts { get; set; }
}
