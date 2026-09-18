using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Permissions;

namespace TaskManagementSystem.Api.Endpoints.Identity.Permissions;

/// <summary>
/// GET /identity/permissions — Lists all permissions.
/// </summary>
public static class ListPermissionsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/permissions", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityRoles.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListPermissionsQuery(), cancellationToken);
        return result.ToHttpResult(permissions => Results.Ok(permissions.Select(IdentityMapping.ToPermissionResponse)));
    }
}
