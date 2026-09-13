using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence;

internal sealed class StepRepository(WorkflowsDbContext context)
    : GenericRepository<WorkflowStep, WorkflowsDbContext>(context), IStepRepository
{
    public Task<WorkflowStep?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(step => step.Id == id && !step.Archived, cancellationToken);

    public Task<WorkflowStep?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(step => step.Id == id && !step.Archived, cancellationToken);

    public async Task<IReadOnlyList<WorkflowStep>> ListActiveByNodeAsync(
        int nodeId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(step => step.NodeId == nodeId && !step.Archived)
            .OrderBy(step => step.Order)
            .ToListAsync(cancellationToken);

    public async Task<int> GetNextOrderAsync(int nodeId, CancellationToken cancellationToken = default)
    {
        var maxOrder = await Set.AsNoTracking()
            .Where(step => step.NodeId == nodeId && !step.Archived)
            .MaxAsync(step => (int?)step.Order, cancellationToken);

        return (maxOrder ?? 0) + 1;
    }

    public Task<bool> NodeExistsActiveAsync(int nodeId, CancellationToken cancellationToken = default) =>
        Context.Nodes.AsNoTracking().AnyAsync(node => node.Id == nodeId && !node.Archived, cancellationToken);

    public Task<bool> TaskBankExistsActiveAsync(int taskBankId, CancellationToken cancellationToken = default) =>
        Context.TaskBank.AsNoTracking().AnyAsync(item => item.Id == taskBankId && item.Active, cancellationToken);

    public Task AddAsync(WorkflowStep step, CancellationToken cancellationToken = default) =>
        AddEntityAsync(step, cancellationToken);
}
