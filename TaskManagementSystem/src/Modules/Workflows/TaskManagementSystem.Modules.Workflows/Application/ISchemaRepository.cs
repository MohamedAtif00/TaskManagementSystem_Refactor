using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Application;

public interface ISchemaRepository
{
    Task<WorkflowSchema?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<WorkflowSchema?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkflowSchema>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<bool> TypeExistsAsync(int typeId, CancellationToken cancellationToken = default);

    Task AddAsync(WorkflowSchema schema, CancellationToken cancellationToken = default);
}
