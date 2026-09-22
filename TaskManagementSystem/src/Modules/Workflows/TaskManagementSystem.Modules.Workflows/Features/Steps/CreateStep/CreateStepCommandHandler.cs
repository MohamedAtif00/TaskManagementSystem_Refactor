using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.CreateStep;

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

        if (!await unitOfWork.Steps.TicketBankExistsActiveAsync(request.TicketBankId, cancellationToken))
        {
            return Result.Fail<StepListItemResult>(WorkflowsErrors.TicketBankNotFound);
        }

        var order = await unitOfWork.Steps.GetNextOrderAsync(request.NodeId, cancellationToken);
        var createResult = WorkflowStep.Create(
            order,
            request.Duration,
            request.Priority,
            request.NodeId,
            request.TicketBankId);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<StepListItemResult>(createResult.Error);
        }

        await unitOfWork.Steps.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(StepListItemResult.From(createResult.Value));
    }
}

