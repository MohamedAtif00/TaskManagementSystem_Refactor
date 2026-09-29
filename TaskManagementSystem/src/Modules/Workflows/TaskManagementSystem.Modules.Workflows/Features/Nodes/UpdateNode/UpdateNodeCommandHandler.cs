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

        var predecessorIds = (request.PredecessorIds ?? [])
            .Where(id => id > 0 && id != node.Id)
            .Distinct()
            .ToList();
        if (!await unitOfWork.Nodes.PredecessorsBelongToSchemaAsync(
                node.SchemaId,
                node.Id,
                predecessorIds,
                cancellationToken))
        {
            return Result.Fail<NodeListItemResult>(WorkflowsErrors.PredecessorInvalid);
        }

        var updateResult = node.Update(request.Name, request.IsStart, request.IsEnd);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<NodeListItemResult>(updateResult.Error);
        }

        await unitOfWork.Nodes.ReplacePredecessorsAsync(node.Id, predecessorIds, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(NodeListItemResult.From(node, predecessorIds));
    }
}

