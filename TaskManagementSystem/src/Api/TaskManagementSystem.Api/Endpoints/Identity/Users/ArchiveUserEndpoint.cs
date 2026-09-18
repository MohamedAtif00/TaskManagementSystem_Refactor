using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Users.ArchiveUser;

namespace TaskManagementSystem.Api.Endpoints.Identity.Users;

/// <summary>
/// DELETE /identity/users/{userId} — Archives a user.
/// </summary>
public static class ArchiveUserEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder users)
    {
        users.MapDelete("/{userId:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityUsers.Delete);
        return users;
    }

    private static async Task<IResult> HandleAsync(
        int userId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveUserCommand(userId), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
