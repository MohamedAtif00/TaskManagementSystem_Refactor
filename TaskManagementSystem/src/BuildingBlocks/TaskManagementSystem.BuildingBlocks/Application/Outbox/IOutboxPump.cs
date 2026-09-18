namespace TaskManagementSystem.BuildingBlocks.Application.Outbox;

public interface IOutboxPump
{
    Task<bool> ProcessBatchAsync(string schema, CancellationToken cancellationToken = default);
}
