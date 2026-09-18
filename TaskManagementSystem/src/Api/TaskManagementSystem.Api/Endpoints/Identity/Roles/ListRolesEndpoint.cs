using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Roles;

namespace TaskManagementSystem.Api.Endpoints.Identity.Roles;

/// <summary>
/// GET /identity/roles — Lists all roles.
/// </summary>
public static class ListRolesEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder roles)
    {
        roles.MapGet("", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityRoles.Read);
        return roles;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListRolesQuery(), cancellationToken);
        return result.ToHttpResult(roles => Results.Ok(roles.Select(IdentityMapping.ToRoleResponse)));
    }
}
