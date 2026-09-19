using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.SchemaTypes.ListSchemaTypes;

public sealed record ListSchemaTypesQuery : IQuery<Result<IReadOnlyList<SchemaTypeResult>>>;

