using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.IntegrationEvents.Identity;
using TaskManagementSystem.Modules.Identity.Domain.Events;

namespace TaskManagementSystem.Modules.Identity.Features.Integration;

internal sealed class UserMetadataChangedDomainEventHandler(IOutboxWriter outboxWriter)
    : INotificationHandler<UserMetadataChangedDomainEvent>
{
    public Task Handle(UserMetadataChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserMetadataChangedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            notification.UserId,
            notification.TeamId,
            notification.TeamleaderId,
            notification.RoleId);

        return outboxWriter.AddAsync(integrationEvent, cancellationToken);
    }
}
