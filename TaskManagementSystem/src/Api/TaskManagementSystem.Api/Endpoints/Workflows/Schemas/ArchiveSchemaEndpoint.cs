using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Workflows.Features.Schemas.ArchiveSchema;

namespace TaskManagementSystem.Api.Endpoints.Workflows.Schemas;

/// <summary>
/// DELETE /workflows/schemas/{id} — Archives a workflow schema.
/// </summary>
public static class ArchiveSchemaEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder schemas)
    {
        schemas.MapDelete("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.Workflows.Delete);

        return schemas;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveSchemaCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
