using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Events;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Events;

public sealed class InProcessIntegrationEventBus(IPublisher publisher) : IIntegrationEventBus
{
    public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default) =>
        publisher.Publish(integrationEvent, cancellationToken);
}
