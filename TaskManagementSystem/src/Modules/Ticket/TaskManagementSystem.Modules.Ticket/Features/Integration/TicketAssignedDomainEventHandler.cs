using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.IntegrationEvents.Ticket;
using TaskManagementSystem.Modules.Ticket.Domain.Events;

namespace TaskManagementSystem.Modules.Ticket.Features.Integration;

internal sealed class TicketAssignedDomainEventHandler(IOutboxWriter outboxWriter)
    : INotificationHandler<TicketAssignedDomainEvent>
{
    public Task Handle(TicketAssignedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new TicketAssignedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            notification.TicketId,
            notification.AssignedUserId);

        return outboxWriter.AddAsync(integrationEvent, cancellationToken);
    }
}
