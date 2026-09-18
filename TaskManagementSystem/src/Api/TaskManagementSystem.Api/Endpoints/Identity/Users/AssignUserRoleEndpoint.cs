using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Users;

namespace TaskManagementSystem.Api.Endpoints.Identity.Users;

/// <summary>
/// PUT /identity/users/{userId}/role — Assigns a role to a user.
/// </summary>
public static class AssignUserRoleEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder users)
    {
        users.MapPut("/{userId:int}/role", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityUsers.Update);
        return users;
    }

    private static async Task<IResult> HandleAsync(
        int userId,
        AssignUserRoleRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AssignUserRoleCommand(userId, request.RoleId), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
