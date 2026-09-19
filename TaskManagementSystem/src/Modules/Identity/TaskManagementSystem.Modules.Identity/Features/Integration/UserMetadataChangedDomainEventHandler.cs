using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.IntegrationEvents.Identity;
using TaskManagementSystem.Modules.Identity.Domain.Events;

namespace TaskManagementSystem.Modules.Identity.Features.Integration;

internal sealed class UserMetadataChangedDomainEventHandler(
    [FromKeyedServices(IntegrationModuleKeys.Identity)] IOutboxWriter outboxWriter)
    : INotificationHandler<UserMetadataChangedDomainEvent>
{
    public Task Handle(UserMetadataChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserMetadataChangedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            notification.UserId,
            notification.TeamId,
            notification.RoleId);

        return outboxWriter.AddAsync(integrationEvent, cancellationToken);
    }
}
