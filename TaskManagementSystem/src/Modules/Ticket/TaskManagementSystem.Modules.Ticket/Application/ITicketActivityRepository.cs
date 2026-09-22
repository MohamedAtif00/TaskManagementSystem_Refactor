using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITicketActivityRepository
{
    Task AddAsync(TicketActivity activity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketActivity>> ListByTicketAsync(
        int ticketId,
        CancellationToken cancellationToken = default);
}
