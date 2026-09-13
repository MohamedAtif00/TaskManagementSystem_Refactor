using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence;

internal sealed class SchemaRepository(WorkflowsDbContext context)
    : GenericRepository<WorkflowSchema, WorkflowsDbContext>(context), ISchemaRepository
{
    public Task<WorkflowSchema?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(schema => schema.Id == id && !schema.Archived, cancellationToken);

    public Task<WorkflowSchema?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(schema => schema.Id == id && !schema.Archived, cancellationToken);

    public async Task<IReadOnlyList<WorkflowSchema>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(schema => !schema.Archived)
            .OrderBy(schema => schema.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> TypeExistsAsync(int typeId, CancellationToken cancellationToken = default) =>
        Context.SchemaTypes.AsNoTracking().AnyAsync(type => type.Id == typeId, cancellationToken);

    public Task AddAsync(WorkflowSchema schema, CancellationToken cancellationToken = default) =>
        AddEntityAsync(schema, cancellationToken);
}
