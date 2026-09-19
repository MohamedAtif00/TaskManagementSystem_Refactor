using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Users.CreateUser;

namespace TaskManagementSystem.Api.Endpoints.Identity.Users;

/// <summary>
/// POST /identity/users — Creates a new user.
/// </summary>
public static class CreateUserEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder users)
    {
        users.MapPost("", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityUsers.Create);
        return users;
    }

    private static async Task<IResult> HandleAsync(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateUserCommand(
                request.Name,
                request.HrCode,
                request.Email,
                request.Phone,
                request.Title,
                request.RoleId,
                (AccountType)request.AccountType,
                request.TeamId),
            cancellationToken);

        return result.ToHttpResult(user => Results.Created($"/identity/users/{user.Id}", IdentityMapping.ToUserDetailResponse(user)));
    }
}
