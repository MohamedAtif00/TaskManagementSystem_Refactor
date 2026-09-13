using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Permissions.ApprovePermissionRequest;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

public static class ApprovePermissionRequestEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder permissions)
    {
        permissions.MapPost("/{id:int}/approve", HandleAsync)
            .RequireAuthorization(nameof(UserRole.Owner));
        return permissions;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new ApprovePermissionRequestCommand(userId, id), cancellationToken);

        return result.ToHttpResult(permission => Results.Ok(PermissionMapping.MapPermission(permission)));
    }
}
