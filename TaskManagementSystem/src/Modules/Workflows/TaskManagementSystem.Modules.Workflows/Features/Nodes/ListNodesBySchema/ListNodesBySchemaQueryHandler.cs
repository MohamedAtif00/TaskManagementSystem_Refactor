using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Workflows.Application;

namespace TaskManagementSystem.Modules.Workflows.Features.Nodes.ListNodesBySchema;

public sealed class ListNodesBySchemaQueryHandler(IWorkflowsUnitOfWork unitOfWork)
    : IRequestHandler<ListNodesBySchemaQuery, Result<IReadOnlyList<NodeListItemResult>>>
{
    public async Task<Result<IReadOnlyList<NodeListItemResult>>> Handle(
        ListNodesBySchemaQuery request,
        CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Nodes.SchemaExistsActiveAsync(request.SchemaId, cancellationToken))
        {
            return Result.Fail<IReadOnlyList<NodeListItemResult>>(WorkflowsErrors.SchemaNotFound);
        }

        var nodes = await unitOfWork.Nodes.ListActiveBySchemaAsync(request.SchemaId, cancellationToken);
        return Result.Ok<IReadOnlyList<NodeListItemResult>>(nodes.Select(NodeListItemResult.From).ToList());
    }
}

