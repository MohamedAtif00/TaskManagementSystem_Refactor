using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.CreateStep;

public sealed record CreateStepCommand(
    int NodeId,
    int TicketBankId,
    int Duration,
    int Priority) : ICommand<Result<StepListItemResult>>;

