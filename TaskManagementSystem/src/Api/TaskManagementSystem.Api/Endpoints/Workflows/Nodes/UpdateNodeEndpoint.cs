using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Nodes.UpdateNode;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Nodes;

/// <summary>
/// PUT /workflows/nodes/{id} — Updates a workflow node.
/// </summary>
public static class UpdateNodeEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder nodes)
    {
        nodes.MapPut("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Update);

        return nodes;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateNodeRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateNodeCommand(id, request.Name, request.IsStart, request.IsEnd),
            cancellationToken);

        return result.ToHttpResult(node => Results.Ok(WorkflowMapping.MapNodeListItem(node)));
    }
}
