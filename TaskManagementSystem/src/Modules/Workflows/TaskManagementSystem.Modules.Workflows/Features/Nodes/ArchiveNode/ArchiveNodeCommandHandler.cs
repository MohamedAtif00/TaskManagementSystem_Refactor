using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.ArchiveNode;

public sealed class ArchiveNodeCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveNodeCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        ArchiveNodeCommand request,
        CancellationToken cancellationToken)
    {
        var node = await unitOfWork.Nodes.GetByIdTrackedAsync(request.NodeId, cancellationToken);
        if (node is null)
        {
            return Result.Fail<NoValue>(WorkflowsErrors.NodeNotFound);
        }

        node.Archive();
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}

