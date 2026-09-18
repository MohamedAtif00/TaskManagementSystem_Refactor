namespace TaskManagementSystem.BuildingBlocks.Application.Events;

public interface IIntegrationEventBus
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
