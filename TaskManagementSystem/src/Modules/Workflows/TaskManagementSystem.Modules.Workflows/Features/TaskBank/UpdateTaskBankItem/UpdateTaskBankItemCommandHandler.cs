using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.TaskBank.UpdateTaskBankItem;

public sealed class UpdateTaskBankItemCommandHandler(
    IWorkflowsUnitOfWork unitOfWork,
    IOrganizationTeamLookup organizationTeamLookup)
    : IRequestHandler<UpdateTaskBankItemCommand, Result<TaskBankListItemResult>>
{
    public async Task<Result<TaskBankListItemResult>> Handle(
        UpdateTaskBankItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await unitOfWork.TaskBank.GetByIdTrackedAsync(request.TaskBankId, cancellationToken);
        if (item is null)
        {
            return Result.Fail<TaskBankListItemResult>(WorkflowsErrors.TaskBankNotFound);
        }

        if (!await organizationTeamLookup.ActiveTeamExistsAsync(request.TeamId, cancellationToken))
        {
            return Result.Fail<TaskBankListItemResult>(WorkflowsErrors.TeamInvalid);
        }

        var updateResult = item.Update(
            request.Name,
            request.Duration,
            request.Type,
            request.TeamLeaderOnly,
            request.TeamId);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<TaskBankListItemResult>(updateResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(TaskBankListItemResult.From(item));
    }
}

