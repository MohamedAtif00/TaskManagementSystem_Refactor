using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Users.UpdateUser;

namespace TaskManagementSystem.Api.Endpoints.Identity.Users;

/// <summary>
/// PUT /identity/users/{userId} — Updates a user.
/// </summary>
public static class UpdateUserEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder users)
    {
        users.MapPut("/{userId:int}", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityUsers.Update);
        return users;
    }

    private static async Task<IResult> HandleAsync(
        int userId,
        UpdateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateUserCommand(
                userId,
                request.Name,
                request.HrCode,
                request.Email,
                request.Phone,
                request.Title,
                request.RoleId,
                (AccountType)request.AccountType,
                request.TeamId,
                request.TeamleaderId),
            cancellationToken);

        return result.ToHttpResult(user => Results.Ok(IdentityMapping.ToUserDetailResponse(user)));
    }
}
