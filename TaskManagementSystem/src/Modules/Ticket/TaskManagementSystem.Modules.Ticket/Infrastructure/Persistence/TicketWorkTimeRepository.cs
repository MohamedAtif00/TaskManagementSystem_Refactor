using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence;

internal sealed class TicketWorkTimeRepository(TicketDbContext context)
    : GenericRepository<TicketWorkTime, TicketDbContext>(context), ITicketWorkTimeRepository
{
    public Task<TicketWorkTime?> GetOpenByTicketAndUserTrackedAsync(
        int ticketId,
        int userId,
        CancellationToken cancellationToken = default) =>
        FindTrackedAsync(
            workTime => workTime.TicketId == ticketId && workTime.UserId == userId && workTime.EndDate == null,
            cancellationToken);

    public Task AddAsync(TicketWorkTime workTime, CancellationToken cancellationToken = default) =>
        AddEntityAsync(workTime, cancellationToken);
}
