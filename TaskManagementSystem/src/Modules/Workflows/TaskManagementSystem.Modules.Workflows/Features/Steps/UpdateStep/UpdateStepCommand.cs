using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.UpdateStep;

public sealed record UpdateStepCommand(
    int StepId,
    int TicketBankId,
    int Duration,
    int Priority) : ICommand<Result<StepListItemResult>>;

