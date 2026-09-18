using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Roles;

namespace TaskManagementSystem.Api.Endpoints.Identity.Roles;

/// <summary>
/// PUT /identity/roles/{id}/permissions — Sets all permissions for a role.
/// </summary>
public static class SetRolePermissionsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder roles)
    {
        roles.MapPut("/{id:int}/permissions", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityRoles.Manage);
        return roles;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        SetRolePermissionsRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SetRolePermissionsCommand(id, request.PermissionIds),
            cancellationToken);

        return result.ToHttpResult(role => Results.Ok(IdentityMapping.ToRoleResponse(role)));
    }
}
