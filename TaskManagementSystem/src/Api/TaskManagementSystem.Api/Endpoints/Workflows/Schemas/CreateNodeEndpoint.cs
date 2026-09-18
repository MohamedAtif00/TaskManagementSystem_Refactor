using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Nodes.CreateNode;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Schemas;

/// <summary>
/// POST /workflows/schemas/{schemaId}/nodes — Creates a node on a workflow schema.
/// </summary>
public static class CreateNodeEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder schemas)
    {
        schemas.MapPost("/{schemaId:int}/nodes", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Create);

        return schemas;
    }

    private static async Task<IResult> HandleAsync(
        int schemaId,
        CreateNodeRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateNodeCommand(schemaId, request.Name, request.IsStart, request.IsEnd),
            cancellationToken);

        return result.ToHttpResult(node =>
            Results.Created($"/workflows/nodes/{node.Id}", WorkflowMapping.MapNodeListItem(node)));
    }
}
