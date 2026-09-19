using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.ListSchemas;

public sealed record ListSchemasQuery : IQuery<Result<IReadOnlyList<SchemaListItemResult>>>;

