using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence;

internal sealed class TaskWorkTimeRepository(TicketDbContext context)
    : GenericRepository<TaskWorkTime, TicketDbContext>(context), ITaskWorkTimeRepository
{
    public Task<TaskWorkTime?> GetOpenByTicketAndUserTrackedAsync(
        int ticketId,
        int userId,
        CancellationToken cancellationToken = default) =>
        FindTrackedAsync(
            workTime => workTime.TaskId == ticketId && workTime.UserId == userId && workTime.EndDate == null,
            cancellationToken);

    public Task AddAsync(TaskWorkTime workTime, CancellationToken cancellationToken = default) =>
        AddEntityAsync(workTime, cancellationToken);
}
