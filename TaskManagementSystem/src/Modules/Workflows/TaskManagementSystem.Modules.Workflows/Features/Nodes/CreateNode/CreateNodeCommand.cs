using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.CreateNode;

public sealed record CreateNodeCommand(
    int SchemaId,
    string Name,
    bool IsStart,
    bool IsEnd) : ICommand<Result<NodeListItemResult>>;

