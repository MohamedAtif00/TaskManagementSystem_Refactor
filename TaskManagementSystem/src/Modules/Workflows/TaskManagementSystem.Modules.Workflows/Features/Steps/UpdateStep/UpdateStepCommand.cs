using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.UpdateStep;

public sealed record UpdateStepCommand(
    int StepId,
    int TaskBankId,
    int Duration,
    int Priority) : ICommand<Result<StepListItemResult>>;

public sealed class UpdateStepCommandValidator : AbstractValidator<UpdateStepCommand>
{
    public UpdateStepCommandValidator()
    {
        RuleFor(x => x.StepId).GreaterThan(0);
        RuleFor(x => x.TaskBankId).GreaterThan(0);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateStepCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<UpdateStepCommand, Result<StepListItemResult>>
{
    public async Task<Result<StepListItemResult>> Handle(
        UpdateStepCommand request,
        CancellationToken cancellationToken)
    {
        var step = await unitOfWork.Steps.GetByIdTrackedAsync(request.StepId, cancellationToken);
        if (step is null)
        {
            return Result.Fail<StepListItemResult>(WorkflowsErrors.StepNotFound);
        }

        if (!await unitOfWork.Steps.TaskBankExistsActiveAsync(request.TaskBankId, cancellationToken))
        {
            return Result.Fail<StepListItemResult>(WorkflowsErrors.TaskBankNotFound);
        }

        var updateResult = step.Update(step.Order, request.Duration, request.Priority, request.TaskBankId);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<StepListItemResult>(updateResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(StepListItemResult.From(step));
    }
}
