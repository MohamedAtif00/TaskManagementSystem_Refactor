using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.SchemaTypes.ListSchemaTypes;

namespace TaskManagementSystem.Api.Endpoints.Workflows.SchemaTypes;

/// <summary>
/// GET /workflows/schema-types — Lists available workflow schema types.
/// </summary>
public static class ListSchemaTypesEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/schema-types", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Read);

        return group;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSchemaTypesQuery(), cancellationToken);
        return result.ToHttpResult(types =>
            Results.Ok(types.Select(WorkflowMapping.MapSchemaType).ToList()));
    }
}
