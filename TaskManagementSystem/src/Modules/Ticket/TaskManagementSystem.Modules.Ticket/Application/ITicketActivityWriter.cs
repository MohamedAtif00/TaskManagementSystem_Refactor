using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITicketActivityWriter
{
    Task WriteAsync(
        int ticketId,
        TicketActivityType type,
        int? actorOneId,
        CancellationToken cancellationToken = default,
        int? actorTwoId = null,
        string? additionalInfo = null);
}
