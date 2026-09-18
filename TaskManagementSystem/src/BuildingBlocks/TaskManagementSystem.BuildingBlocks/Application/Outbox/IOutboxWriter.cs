namespace TaskManagementSystem.BuildingBlocks.Application.Outbox;

public interface IOutboxWriter
{
    Task AddAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
