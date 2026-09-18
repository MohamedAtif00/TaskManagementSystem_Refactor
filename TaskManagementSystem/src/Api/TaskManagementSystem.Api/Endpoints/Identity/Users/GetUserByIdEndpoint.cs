using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Users.GetUserById;

namespace TaskManagementSystem.Api.Endpoints.Identity.Users;

/// <summary>
/// GET /identity/users/{userId} — Gets a user by ID.
/// </summary>
public static class GetUserByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder users)
    {
        users.MapGet("/{userId:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityUsers.Read);
        return users;
    }

    private static async Task<IResult> HandleAsync(
        int userId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery(userId), cancellationToken);
        return result.ToHttpResult(user => Results.Ok(IdentityMapping.ToUserDetailResponse(user)));
    }
}
