using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Roles;

namespace TaskManagementSystem.Api.Endpoints.Identity.Roles;

/// <summary>
/// DELETE /identity/roles/{id}/permissions/{permissionId} — Removes a permission from a role.
/// </summary>
public static class RemoveRolePermissionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder roles)
    {
        roles.MapDelete("/{id:int}/permissions/{permissionId:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityRoles.Manage);
        return roles;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        int permissionId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveRolePermissionCommand(id, permissionId), cancellationToken);
        return result.ToHttpResult(role => Results.Ok(IdentityMapping.ToRoleResponse(role)));
    }
}
