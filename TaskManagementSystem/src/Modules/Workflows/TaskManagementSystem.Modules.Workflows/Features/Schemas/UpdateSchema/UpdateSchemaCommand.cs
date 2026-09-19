using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.UpdateSchema;

public sealed record UpdateSchemaCommand(
    int SchemaId,
    string Name,
    string Description,
    int? TypeId) : ICommand<Result<SchemaDetailResult>>;

