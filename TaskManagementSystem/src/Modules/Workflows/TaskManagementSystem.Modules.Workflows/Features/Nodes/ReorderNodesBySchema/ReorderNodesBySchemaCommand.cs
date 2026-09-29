using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.ReorderNodesBySchema;

public sealed record ReorderNodesBySchemaCommand(int SchemaId, IReadOnlyList<int> OrderedNodeIds)
    : ICommand<Result<NoValue>>;
