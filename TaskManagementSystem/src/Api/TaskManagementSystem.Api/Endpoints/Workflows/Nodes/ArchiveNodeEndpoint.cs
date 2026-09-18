using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Nodes.ArchiveNode;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Nodes;

/// <summary>
/// DELETE /workflows/nodes/{id} — Archives a workflow node.
/// </summary>
public static class ArchiveNodeEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder nodes)
    {
        nodes.MapDelete("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Delete);

        return nodes;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveNodeCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
