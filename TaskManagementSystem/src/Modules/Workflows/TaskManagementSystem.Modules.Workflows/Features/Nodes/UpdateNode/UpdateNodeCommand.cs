using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.UpdateNode;

public sealed record UpdateNodeCommand(
    int NodeId,
    string Name,
    bool IsStart,
    bool IsEnd) : ICommand<Result<NodeListItemResult>>;

