using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Domain;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.CreateNode;

public sealed class CreateNodeCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<CreateNodeCommand, Result<NodeListItemResult>>
{
    public async Task<Result<NodeListItemResult>> Handle(
        CreateNodeCommand request,
        CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Nodes.SchemaExistsActiveAsync(request.SchemaId, cancellationToken))
        {
            return Result.Fail<NodeListItemResult>(WorkflowsErrors.SchemaNotFound);
        }

        var order = await unitOfWork.Nodes.GetNextOrderAsync(request.SchemaId, cancellationToken);
        var createResult = WorkflowNode.Create(
            request.Name,
            order,
            request.IsStart,
            request.IsEnd,
            request.SchemaId);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<NodeListItemResult>(createResult.Error);
        }

        await unitOfWork.Nodes.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(NodeListItemResult.From(createResult.Value));
    }
}

