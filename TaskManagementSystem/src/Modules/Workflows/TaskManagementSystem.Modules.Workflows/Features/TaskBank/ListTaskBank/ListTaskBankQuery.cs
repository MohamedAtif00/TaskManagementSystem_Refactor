using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.TaskBank.ListTaskBank;

public sealed record ListTaskBankQuery : IQuery<Result<IReadOnlyList<TaskBankListItemResult>>>;

