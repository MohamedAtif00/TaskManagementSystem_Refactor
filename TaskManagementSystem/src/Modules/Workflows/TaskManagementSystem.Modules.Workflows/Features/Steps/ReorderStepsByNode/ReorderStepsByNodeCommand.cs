using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.ReorderStepsByNode;

public sealed record ReorderStepsByNodeCommand(int NodeId, IReadOnlyList<int> OrderedStepIds)
    : ICommand<Result<NoValue>>;
