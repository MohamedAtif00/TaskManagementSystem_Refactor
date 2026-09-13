using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.CreateStep;

public sealed record CreateStepCommand(
    int NodeId,
    int TaskBankId,
    int Duration,
    int Priority) : ICommand<Result<StepListItemResult>>;

public sealed class CreateStepCommandValidator : AbstractValidator<CreateStepCommand>
{
    public CreateStepCommandValidator()
    {
        RuleFor(x => x.NodeId).GreaterThan(0);
        RuleFor(x => x.TaskBankId).GreaterThan(0);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateStepCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<CreateStepCommand, Result<StepListItemResult>>
{
    public async Task<Result<StepListItemResult>> Handle(
        CreateStepCommand request,
        CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Steps.NodeExistsActiveAsync(request.NodeId, cancellationToken))
        {
            return Result.Fail<StepListItemResult>(WorkflowsErrors.NodeNotFound);
        }

        if (!await unitOfWork.Steps.TaskBankExistsActiveAsync(request.TaskBankId, cancellationToken))
        {
            return Result.Fail<StepListItemResult>(WorkflowsErrors.TaskBankNotFound);
        }

        var order = await unitOfWork.Steps.GetNextOrderAsync(request.NodeId, cancellationToken);
        var createResult = WorkflowStep.Create(
            order,
            request.Duration,
            request.Priority,
            request.NodeId,
            request.TaskBankId);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<StepListItemResult>(createResult.Error);
        }

        await unitOfWork.Steps.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(StepListItemResult.From(createResult.Value));
    }
}
