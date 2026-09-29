using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Steps.ReorderStepsByNode;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Nodes;

public static class ReorderStepsByNodeEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder nodes)
    {
        nodes.MapPut("/{nodeId:int}/steps/reorder", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Update);

        return nodes;
    }

    private static async Task<IResult> HandleAsync(
        int nodeId,
        ReorderStepsRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ReorderStepsByNodeCommand(nodeId, request.OrderedStepIds),
            cancellationToken);

        return result.ToHttpResult(_ => Results.NoContent());
    }
}
