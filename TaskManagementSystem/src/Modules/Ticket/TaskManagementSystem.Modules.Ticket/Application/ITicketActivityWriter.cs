using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITicketActivityWriter
{
    Task WriteAsync(
        int ticketId,
        TicketActivityType type,
        string message,
        int? actorUserId,
        CancellationToken cancellationToken = default);
}
