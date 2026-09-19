using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.CreateSchema;

public sealed record CreateSchemaCommand(
    string Name,
    string Description,
    int? TypeId) : ICommand<Result<SchemaDetailResult>>;

