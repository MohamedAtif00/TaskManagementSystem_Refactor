using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Nodes.ReorderNodesBySchema;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Schemas;

public static class ReorderNodesBySchemaEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder schemas)
    {
        schemas.MapPut("/{schemaId:int}/nodes/reorder", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Update);

        return schemas;
    }

    private static async Task<IResult> HandleAsync(
        int schemaId,
        ReorderNodesRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ReorderNodesBySchemaCommand(schemaId, request.OrderedNodeIds),
            cancellationToken);

        return result.ToHttpResult(_ => Results.NoContent());
    }
}
