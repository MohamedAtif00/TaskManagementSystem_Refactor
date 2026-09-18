using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Roles;

namespace TaskManagementSystem.Api.Endpoints.Identity.Roles;

/// <summary>
/// DELETE /identity/roles/{id} — Deletes a role.
/// </summary>
public static class DeleteRoleEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder roles)
    {
        roles.MapDelete("/{id:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityRoles.Delete);
        return roles;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteRoleCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
