using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Application;

public interface INodeRepository
{
    Task<WorkflowNode?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<WorkflowNode?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkflowNode>> ListActiveBySchemaAsync(int schemaId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkflowNode>> ListActiveBySchemaTrackedAsync(
        int schemaId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextOrderAsync(int schemaId, CancellationToken cancellationToken = default);

    Task<bool> SchemaExistsActiveAsync(int schemaId, CancellationToken cancellationToken = default);

    Task AddAsync(WorkflowNode node, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, IReadOnlyList<int>>> ListPredecessorIdsAsync(
        IReadOnlyCollection<int> nodeIds,
        CancellationToken cancellationToken = default);

    Task<bool> PredecessorsBelongToSchemaAsync(
        int schemaId,
        int nodeId,
        IReadOnlyCollection<int> predecessorIds,
        CancellationToken cancellationToken = default);

    Task ReplacePredecessorsAsync(
        int nodeId,
        IReadOnlyCollection<int> predecessorIds,
        CancellationToken cancellationToken = default);
}
