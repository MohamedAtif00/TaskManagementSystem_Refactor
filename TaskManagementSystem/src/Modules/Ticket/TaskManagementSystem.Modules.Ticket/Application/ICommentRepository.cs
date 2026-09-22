using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ICommentRepository
{
    Task<IReadOnlyList<Comment>> ListByTicketAsync(int ticketId, CancellationToken cancellationToken = default);

    Task<Comment?> GetByIdTrackedAsync(int commentId, CancellationToken cancellationToken = default);

    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
}
