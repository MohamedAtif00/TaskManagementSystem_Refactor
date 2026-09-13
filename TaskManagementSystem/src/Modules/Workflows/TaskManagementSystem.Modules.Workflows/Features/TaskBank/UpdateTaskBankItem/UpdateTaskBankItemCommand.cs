using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;
namespace TaskManagementSystem.Modules.Workflows.Features.TaskBank.UpdateTaskBankItem;

public sealed record UpdateTaskBankItemCommand(
    int TaskBankId,
    string Name,
    int Duration,
    TaskBankType Type,
    bool TeamLeaderOnly,
    int TeamId) : ICommand<Result<TaskBankListItemResult>>;

public sealed class UpdateTaskBankItemCommandValidator : AbstractValidator<UpdateTaskBankItemCommand>
{
    public UpdateTaskBankItemCommandValidator()
    {
        RuleFor(x => x.TaskBankId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TeamId).GreaterThan(0);
    }
}

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
