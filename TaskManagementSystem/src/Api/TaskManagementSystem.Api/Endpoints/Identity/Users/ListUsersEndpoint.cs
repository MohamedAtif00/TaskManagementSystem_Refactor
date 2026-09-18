using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Users.ListUsers;

namespace TaskManagementSystem.Api.Endpoints.Identity.Users;

/// <summary>
/// GET /identity/users — Lists all users.
/// </summary>
public static class ListUsersEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder users)
    {
        users.MapGet("", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityUsers.Read);
        return users;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListUsersQuery(), cancellationToken);
        return result.ToHttpResult(users => Results.Ok(users.Select(IdentityMapping.ToUserListItemResponse)));
    }
}
