using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.TaskBank.CreateTaskBankItem;

public sealed class CreateTaskBankItemCommandHandler(
    IWorkflowsUnitOfWork unitOfWork,
    IOrganizationTeamLookup organizationTeamLookup)
    : IRequestHandler<CreateTaskBankItemCommand, Result<TaskBankListItemResult>>
{
    public async Task<Result<TaskBankListItemResult>> Handle(
        CreateTaskBankItemCommand request,
        CancellationToken cancellationToken)
    {
        if (!await organizationTeamLookup.ActiveTeamExistsAsync(request.TeamId, cancellationToken))
        {
            return Result.Fail<TaskBankListItemResult>(WorkflowsErrors.TeamInvalid);
        }

        var createResult = TaskBankItem.Create(
            request.Name,
            request.Duration,
            request.Type,
            request.TeamLeaderOnly,
            request.TeamId);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<TaskBankListItemResult>(createResult.Error);
        }

        await unitOfWork.TaskBank.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(TaskBankListItemResult.From(createResult.Value));
    }
}

