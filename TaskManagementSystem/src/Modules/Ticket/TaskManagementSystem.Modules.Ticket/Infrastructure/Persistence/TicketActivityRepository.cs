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

    public async Task<(IReadOnlyList<TicketActivity> Items, int TotalCount)> ListByTicketPagedAsync(
        int ticketId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking().Where(activity => activity.TicketId == ticketId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(activity => activity.TimeStamp)
            .ThenByDescending(activity => activity.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
