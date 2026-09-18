using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Steps.ListStepsByNode;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Nodes;

/// <summary>
/// GET /workflows/nodes/{nodeId}/steps — Lists steps for a workflow node.
/// </summary>
public static class ListStepsByNodeEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder nodes)
    {
        nodes.MapGet("/{nodeId:int}/steps", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Read);

        return nodes;
    }

    private static async Task<IResult> HandleAsync(
        int nodeId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListStepsByNodeQuery(nodeId), cancellationToken);
        return result.ToHttpResult(steps =>
            Results.Ok(steps.Select(WorkflowMapping.MapStepListItem).ToList()));
    }
}
