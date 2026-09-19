using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Schemas.ArchiveSchema;

public sealed record ArchiveSchemaCommand(int SchemaId) : ICommand<Result<NoValue>>;

