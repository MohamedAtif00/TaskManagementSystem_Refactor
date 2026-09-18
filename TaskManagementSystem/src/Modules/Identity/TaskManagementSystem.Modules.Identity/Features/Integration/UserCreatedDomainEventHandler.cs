using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.IntegrationEvents.Identity;
using TaskManagementSystem.Modules.Identity.Domain.Events;

namespace TaskManagementSystem.Modules.Identity.Features.Integration;

internal sealed class UserCreatedDomainEventHandler(IOutboxWriter outboxWriter)
    : INotificationHandler<UserCreatedDomainEvent>
{
    public Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserCreatedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            notification.UserId,
            notification.TeamId,
            notification.TeamleaderId,
            notification.RoleId,
            notification.AnnualLeave,
            notification.AnnualLeaveMax,
            notification.EmergencyLeave,
            notification.EmergencyLeaveMax,
            notification.SickLeave,
            notification.PermissionBalance,
            notification.PermissionMax,
            notification.WorkFromHome,
            notification.WorkFromHomeMax,
            notification.FromNextBalanceDaysUsed,
            notification.OldAnnualBalance);

        return outboxWriter.AddAsync(integrationEvent, cancellationToken);
    }
}
