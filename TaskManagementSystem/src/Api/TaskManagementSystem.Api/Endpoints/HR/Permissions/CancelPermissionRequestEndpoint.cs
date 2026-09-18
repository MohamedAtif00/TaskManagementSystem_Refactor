using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Permissions.CancelPermissionRequest;

namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

public static class CancelPermissionRequestEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder permissions)
    {
        permissions.MapPut("/{id:int}/cancel", HandleAsync).RequirePermissionCode(PermissionCodes.HrTimeoff.Update);
        return permissions;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new CancelPermissionRequestCommand(userId, id), cancellationToken);

        return result.ToHttpResult(permission => Results.Ok(PermissionMapping.MapPermission(permission)));
    }
}
