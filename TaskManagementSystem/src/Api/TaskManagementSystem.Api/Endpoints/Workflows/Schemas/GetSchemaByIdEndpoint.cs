using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.GetSchemaById;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Schemas;

/// <summary>
/// GET /workflows/schemas/{id} — Gets a workflow schema by id.
/// </summary>
public static class GetSchemaByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder schemas)
    {
        schemas.MapGet("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Read);

        return schemas;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSchemaByIdQuery(id), cancellationToken);
        return result.ToHttpResult(schema => Results.Ok(WorkflowMapping.MapSchemaDetail(schema)));
    }
}
