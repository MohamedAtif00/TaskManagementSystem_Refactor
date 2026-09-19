using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.ArchiveNode;

public sealed record ArchiveNodeCommand(int NodeId) : ICommand<Result<NoValue>>;

