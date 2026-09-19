using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.UpdateNode;

public sealed class UpdateNodeCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<UpdateNodeCommand, Result<NodeListItemResult>>
{
    public async Task<Result<NodeListItemResult>> Handle(
        UpdateNodeCommand request,
        CancellationToken cancellationToken)
    {
        var node = await unitOfWork.Nodes.GetByIdTrackedAsync(request.NodeId, cancellationToken);
        if (node is null)
        {
            return Result.Fail<NodeListItemResult>(WorkflowsErrors.NodeNotFound);
        }

        var updateResult = node.Update(request.Name, request.IsStart, request.IsEnd);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<NodeListItemResult>(updateResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(NodeListItemResult.From(node));
    }
}

