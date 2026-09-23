using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITicketWorkTimeRepository
{
    Task<TicketWorkTime?> GetOpenByTicketAndUserTrackedAsync(
        int ticketId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketWorkTime>> ListOpenByTicketTrackedAsync(
        int ticketId,
        CancellationToken cancellationToken = default);

    Task AddAsync(TicketWorkTime workTime, CancellationToken cancellationToken = default);
}
