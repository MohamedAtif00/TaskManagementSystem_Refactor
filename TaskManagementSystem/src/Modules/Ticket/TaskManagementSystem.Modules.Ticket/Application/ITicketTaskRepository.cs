using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Application;

public interface ITicketTaskRepository
{
    Task<TicketTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TicketTask?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketTask>> ListByLearningObjectiveAsync(
        int learningObjectiveId,
        CancellationToken cancellationToken = default);

    Task AddAsync(TicketTask ticketTask, CancellationToken cancellationToken = default);
}
