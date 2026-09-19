using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.TaskBank.DeactivateTaskBankItem;

public sealed record DeactivateTaskBankItemCommand(int TaskBankId) : ICommand<Result<NoValue>>;

