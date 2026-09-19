using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.GetSchemaById;

public sealed record GetSchemaByIdQuery(int SchemaId) : IQuery<Result<SchemaDetailResult>>;

