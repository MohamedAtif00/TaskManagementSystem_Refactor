using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.TaskBank.ListTaskBank;

public sealed class ListTaskBankQueryHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<ListTaskBankQuery, Result<IReadOnlyList<TaskBankListItemResult>>>
{
    public async Task<Result<IReadOnlyList<TaskBankListItemResult>>> Handle(
        ListTaskBankQuery request,
        CancellationToken cancellationToken)
    {
        var items = await unitOfWork.TaskBank.ListActiveAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<TaskBankListItemResult>>(items.Select(TaskBankListItemResult.From).ToList());
    }
}

