using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.ReorderNodesBySchema;

public sealed class ReorderNodesBySchemaCommandHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<ReorderNodesBySchemaCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        ReorderNodesBySchemaCommand request,
        CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Nodes.SchemaExistsActiveAsync(request.SchemaId, cancellationToken))
        {
            return Result.Fail<NoValue>(WorkflowsErrors.SchemaNotFound);
        }

        var nodes = await unitOfWork.Nodes.ListActiveBySchemaTrackedAsync(request.SchemaId, cancellationToken);
        var validation = ReorderValidation.Validate(request.OrderedNodeIds, nodes.Select(node => node.Id).ToList());
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var byId = nodes.ToDictionary(node => node.Id);
        for (var index = 0; index < request.OrderedNodeIds.Count; index++)
        {
            byId[request.OrderedNodeIds[index]].SetOrder(index + 1);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
