using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Application;

public interface ITaskBankRepository
{
    Task<TaskBankItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TaskBankItem?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskBankItem>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(TaskBankItem item, CancellationToken cancellationToken = default);
}
