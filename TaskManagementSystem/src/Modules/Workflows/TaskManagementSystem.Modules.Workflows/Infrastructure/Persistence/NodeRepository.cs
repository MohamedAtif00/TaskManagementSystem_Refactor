using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence;

internal sealed class NodeRepository(WorkflowsDbContext context)
    : GenericRepository<WorkflowNode, WorkflowsDbContext>(context), INodeRepository
{
    public Task<WorkflowNode?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(node => node.Id == id && !node.Archived, cancellationToken);

    public Task<WorkflowNode?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(node => node.Id == id && !node.Archived, cancellationToken);

    public async Task<IReadOnlyList<WorkflowNode>> ListActiveBySchemaAsync(
        int schemaId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(node => node.SchemaId == schemaId && !node.Archived)
            .OrderBy(node => node.Order)
            .ToListAsync(cancellationToken);

    public async Task<int> GetNextOrderAsync(int schemaId, CancellationToken cancellationToken = default)
    {
        var maxOrder = await Set.AsNoTracking()
            .Where(node => node.SchemaId == schemaId && !node.Archived)
            .MaxAsync(node => (int?)node.Order, cancellationToken);

        return (maxOrder ?? 0) + 1;
    }

    public Task<bool> SchemaExistsActiveAsync(int schemaId, CancellationToken cancellationToken = default) =>
        Context.Schemas.AsNoTracking().AnyAsync(schema => schema.Id == schemaId && !schema.Archived, cancellationToken);

    public Task AddAsync(WorkflowNode node, CancellationToken cancellationToken = default) =>
        AddEntityAsync(node, cancellationToken);
}
