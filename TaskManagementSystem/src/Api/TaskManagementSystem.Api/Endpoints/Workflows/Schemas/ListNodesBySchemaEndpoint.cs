using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Nodes.ListNodesBySchema;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Schemas;

/// <summary>
/// GET /workflows/schemas/{schemaId}/nodes — Lists nodes for a workflow schema.
/// </summary>
public static class ListNodesBySchemaEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder schemas)
    {
        schemas.MapGet("/{schemaId:int}/nodes", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Read);

        return schemas;
    }

    private static async Task<IResult> HandleAsync(
        int schemaId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListNodesBySchemaQuery(schemaId), cancellationToken);
        return result.ToHttpResult(nodes =>
            Results.Ok(nodes.Select(WorkflowMapping.MapNodeListItem).ToList()));
    }
}
