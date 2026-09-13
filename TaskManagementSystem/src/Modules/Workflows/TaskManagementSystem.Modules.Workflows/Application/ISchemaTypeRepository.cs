using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Application;

public interface ISchemaTypeRepository
{
    Task<IReadOnlyList<SchemaType>> ListAsync(CancellationToken cancellationToken = default);
}
