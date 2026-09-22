using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence;

internal sealed class TicketActivityRepository(TicketDbContext context)
    : GenericRepository<TicketActivity, TicketDbContext>(context), ITicketActivityRepository
{
    public Task AddAsync(TicketActivity activity, CancellationToken cancellationToken = default) =>
        AddEntityAsync(activity, cancellationToken);

    public async Task<IReadOnlyList<TicketActivity>> ListByTicketAsync(
        int ticketId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(activity => activity.TicketId == ticketId)
            .OrderByDescending(activity => activity.TimeStamp)
            .ThenByDescending(activity => activity.Id)
            .ToListAsync(cancellationToken);
}
