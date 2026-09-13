using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence;

internal sealed class SchemaTypeRepository(WorkflowsDbContext context) : ISchemaTypeRepository
{
    public async Task<IReadOnlyList<SchemaType>> ListAsync(CancellationToken cancellationToken = default) =>
        await context.SchemaTypes
            .AsNoTracking()
            .OrderBy(type => type.Name)
            .ToListAsync(cancellationToken);
}
