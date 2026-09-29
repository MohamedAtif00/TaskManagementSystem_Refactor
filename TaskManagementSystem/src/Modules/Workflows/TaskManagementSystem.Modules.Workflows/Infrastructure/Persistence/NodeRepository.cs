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

    public async Task<IReadOnlyList<WorkflowNode>> ListActiveBySchemaTrackedAsync(
        int schemaId,
        CancellationToken cancellationToken = default) =>
        await Set
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

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<int>>> ListPredecessorIdsAsync(
        IReadOnlyCollection<int> nodeIds,
        CancellationToken cancellationToken = default)
    {
        if (nodeIds.Count == 0)
        {
            return new Dictionary<int, IReadOnlyList<int>>();
        }

        var rows = await Context.NodeSequences.AsNoTracking()
            .Where(link => nodeIds.Contains(link.NextId))
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(link => link.NextId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<int>)group.Select(link => link.PreviousId).Distinct().ToList());
    }

    public async Task<bool> PredecessorsBelongToSchemaAsync(
        int schemaId,
        int nodeId,
        IReadOnlyCollection<int> predecessorIds,
        CancellationToken cancellationToken = default)
    {
        var ids = predecessorIds.Where(id => id != nodeId).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return predecessorIds.All(id => id != nodeId);
        }

        var matches = await Context.Nodes.AsNoTracking()
            .CountAsync(
                node => ids.Contains(node.Id) && node.SchemaId == schemaId && !node.Archived,
                cancellationToken);
        return matches == ids.Length;
    }

    public async Task ReplacePredecessorsAsync(
        int nodeId,
        IReadOnlyCollection<int> predecessorIds,
        CancellationToken cancellationToken = default)
    {
        var existing = await Context.NodeSequences
            .Where(link => link.NextId == nodeId)
            .ToListAsync(cancellationToken);
        Context.NodeSequences.RemoveRange(existing);

        foreach (var previousId in predecessorIds.Where(id => id != nodeId).Distinct())
        {
            Context.NodeSequences.Add(new NodeSequence { NextId = nodeId, PreviousId = previousId });
        }
    }
}
