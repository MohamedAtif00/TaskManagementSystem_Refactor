using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITicketRepository
{
    Task<Domain.Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Domain.Ticket?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Ticket>> ListByLearningObjectiveAsync(
        int learningObjectiveId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Domain.Ticket ticketTask, CancellationToken cancellationToken = default);
}
