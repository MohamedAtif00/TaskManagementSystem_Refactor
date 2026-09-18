using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Roles;

namespace TaskManagementSystem.Api.Endpoints.Identity.Roles;

/// <summary>
/// POST /identity/roles — Creates a new role.
/// </summary>
public static class CreateRoleEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder roles)
    {
        roles.MapPost("", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityRoles.Create);
        return roles;
    }

    private static async Task<IResult> HandleAsync(
        CreateRoleRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateRoleCommand(request.Name, request.Description, request.PermissionIds),
            cancellationToken);

        return result.ToHttpResult(role => Results.Created($"/identity/roles/{role.Id}", IdentityMapping.ToRoleResponse(role)));
    }
}
