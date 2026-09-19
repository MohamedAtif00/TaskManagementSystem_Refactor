using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.ListStepsByNode;

public sealed record ListStepsByNodeQuery(int NodeId) : IQuery<Result<IReadOnlyList<StepListItemResult>>>;

