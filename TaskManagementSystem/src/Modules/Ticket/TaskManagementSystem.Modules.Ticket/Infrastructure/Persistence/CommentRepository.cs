using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence;

internal sealed class CommentRepository(TicketDbContext context)
    : GenericRepository<Comment, TicketDbContext>(context), ICommentRepository
{
    public async Task<IReadOnlyList<Comment>> ListByTicketAsync(
        int ticketId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(comment => comment.TicketId == ticketId && !comment.Archived)
            .OrderBy(comment => comment.Timestamp)
            .ThenBy(comment => comment.Id)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Comment> Items, int TotalCount)> ListByTicketPagedAsync(
        int ticketId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
            .Where(comment => comment.TicketId == ticketId && !comment.Archived);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(comment => comment.Timestamp)
            .ThenBy(comment => comment.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Comment?> GetByIdTrackedAsync(int commentId, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(comment => comment.Id == commentId, cancellationToken);

    public Task AddAsync(Comment comment, CancellationToken cancellationToken = default) =>
        AddEntityAsync(comment, cancellationToken);
}
