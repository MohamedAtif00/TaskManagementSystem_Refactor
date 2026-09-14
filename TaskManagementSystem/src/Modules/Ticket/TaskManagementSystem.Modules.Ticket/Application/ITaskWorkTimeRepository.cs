using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITaskWorkTimeRepository
{
    Task<TaskWorkTime?> GetOpenByTicketAndUserTrackedAsync(
        int ticketId,
        int userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(TaskWorkTime workTime, CancellationToken cancellationToken = default);
}
