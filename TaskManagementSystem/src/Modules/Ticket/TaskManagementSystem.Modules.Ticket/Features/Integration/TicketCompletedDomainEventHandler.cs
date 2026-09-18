using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;
using TaskManagementSystem.IntegrationEvents.Ticket;
using TaskManagementSystem.Modules.Ticket.Domain.Events;

namespace TaskManagementSystem.Modules.Ticket.Features.Integration;

internal sealed class TicketCompletedDomainEventHandler(IOutboxWriter outboxWriter)
    : INotificationHandler<TicketCompletedDomainEvent>
{
    public Task Handle(TicketCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new TicketCompletedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            notification.TicketId,
            notification.AssignedUserId);

        return outboxWriter.AddAsync(integrationEvent, cancellationToken);
    }
}
