using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.ListSchemas;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Schemas;

/// <summary>
/// GET /workflows/schemas — Lists all workflow schemas.
/// </summary>
public static class ListSchemasEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder schemas)
    {
        schemas.MapGet("", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Read);

        return schemas;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSchemasQuery(), cancellationToken);
        return result.ToHttpResult(schemas =>
            Results.Ok(schemas.Select(WorkflowMapping.MapSchemaListItem).ToList()));
    }
}
