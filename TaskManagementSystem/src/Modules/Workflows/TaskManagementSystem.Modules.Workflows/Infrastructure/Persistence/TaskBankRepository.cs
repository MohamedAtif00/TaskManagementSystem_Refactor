using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence;

internal sealed class TaskBankRepository(WorkflowsDbContext context)
    : GenericRepository<TaskBankItem, WorkflowsDbContext>(context), ITaskBankRepository
{
    public Task<TaskBankItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(item => item.Id == id && item.Active, cancellationToken);

    public Task<TaskBankItem?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(item => item.Id == id && item.Active, cancellationToken);

    public async Task<IReadOnlyList<TaskBankItem>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(item => item.Active)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsActiveByIdAsync(int id, CancellationToken cancellationToken = default) =>
        ExistsReadOnlyAsync(item => item.Id == id && item.Active, cancellationToken);

    public Task AddAsync(TaskBankItem item, CancellationToken cancellationToken = default) =>
        AddEntityAsync(item, cancellationToken);
}
