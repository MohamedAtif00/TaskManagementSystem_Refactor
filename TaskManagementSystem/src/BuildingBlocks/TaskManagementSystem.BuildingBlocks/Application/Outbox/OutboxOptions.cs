namespace TaskManagementSystem.BuildingBlocks.Application.Outbox;

public sealed class OutboxOptions
{
    public int MaxAttempts { get; init; } = 5;

    public TimeSpan PollInterval { get; init; } = TimeSpan.FromSeconds(2);
}
