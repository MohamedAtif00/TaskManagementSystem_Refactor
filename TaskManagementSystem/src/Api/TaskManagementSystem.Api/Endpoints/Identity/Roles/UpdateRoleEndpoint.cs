using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Roles;

namespace TaskManagementSystem.Api.Endpoints.Identity.Roles;

/// <summary>
/// PUT /identity/roles/{id} — Updates a role.
/// </summary>
public static class UpdateRoleEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder roles)
    {
        roles.MapPut("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityRoles.Update);
        return roles;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateRoleRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateRoleCommand(id, request.Name, request.Description),
            cancellationToken);

        return result.ToHttpResult(role => Results.Ok(IdentityMapping.ToRoleResponse(role)));
    }
}
