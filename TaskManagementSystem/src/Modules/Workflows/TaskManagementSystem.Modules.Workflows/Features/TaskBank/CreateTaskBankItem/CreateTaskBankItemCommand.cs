using FluentValidation;
using MediatR;
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

public sealed class CreateTaskBankItemCommandValidator : AbstractValidator<CreateTaskBankItemCommand>
{
    public CreateTaskBankItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TeamId).GreaterThan(0);
    }
}

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
