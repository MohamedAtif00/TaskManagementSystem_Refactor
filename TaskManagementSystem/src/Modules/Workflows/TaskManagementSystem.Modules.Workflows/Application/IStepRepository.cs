using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Application;

public interface IStepRepository
{
    Task<WorkflowStep?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<WorkflowStep?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkflowStep>> ListActiveByNodeAsync(int nodeId, CancellationToken cancellationToken = default);

    Task<int> GetNextOrderAsync(int nodeId, CancellationToken cancellationToken = default);

    Task<bool> NodeExistsActiveAsync(int nodeId, CancellationToken cancellationToken = default);

    Task<bool> TicketBankExistsActiveAsync(int taskBankId, CancellationToken cancellationToken = default);

    Task AddAsync(WorkflowStep step, CancellationToken cancellationToken = default);
}
