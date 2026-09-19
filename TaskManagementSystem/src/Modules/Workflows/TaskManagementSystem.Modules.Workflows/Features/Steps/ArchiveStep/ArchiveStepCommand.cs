using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.ArchiveStep;

public sealed record ArchiveStepCommand(int StepId) : ICommand<Result<NoValue>>;

