using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Roles;

namespace TaskManagementSystem.Api.Endpoints.Identity.Roles;

/// <summary>
/// POST /identity/roles/{id}/permissions/{permissionId} — Adds a permission to a role.
/// </summary>
public static class AddRolePermissionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder roles)
    {
        roles.MapPost("/{id:int}/permissions/{permissionId:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityRoles.Manage);
        return roles;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        int permissionId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AddRolePermissionCommand(id, permissionId), cancellationToken);
        return result.ToHttpResult(role => Results.Ok(IdentityMapping.ToRoleResponse(role)));
    }
}
