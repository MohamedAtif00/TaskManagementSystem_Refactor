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
            .Where(comment => comment.TaskId == ticketId && !comment.Archived)
            .OrderBy(comment => comment.Timestamp)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Comment comment, CancellationToken cancellationToken = default) =>
        AddEntityAsync(comment, cancellationToken);
}
