using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.TaskBank.CreateTaskBankItem;

public sealed record CreateTaskBankItemCommand(
    string Name,
    int Duration,
    TaskBankType Type,
    bool TeamLeaderOnly,
    int TeamId) : ICommand<Result<TaskBankListItemResult>>;

