using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.ReorderStepsByNode;

public sealed class ReorderStepsByNodeCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<ReorderStepsByNodeCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        ReorderStepsByNodeCommand request,
        CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Steps.NodeExistsActiveAsync(request.NodeId, cancellationToken))
        {
            return Result.Fail<NoValue>(WorkflowsErrors.NodeNotFound);
        }

        var steps = await unitOfWork.Steps.ListActiveByNodeTrackedAsync(request.NodeId, cancellationToken);
        var validation = ReorderValidation.Validate(request.OrderedStepIds, steps.Select(step => step.Id).ToList());
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var byId = steps.ToDictionary(step => step.Id);
        for (var index = 0; index < request.OrderedStepIds.Count; index++)
        {
            byId[request.OrderedStepIds[index]].SetOrder(index + 1);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
