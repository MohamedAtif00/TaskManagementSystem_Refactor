using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Permissions.GetPermissionRequests;

namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

public static class GetPermissionRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder permissions)
    {
        permissions.MapGet("", HandleAsync).RequirePermissionCode(PermissionCodes.HrTimeoff.Read);
        return permissions;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new GetPermissionRequestsQuery(userId), cancellationToken);

        return result.ToHttpResult(list =>
            Results.Ok(list.Select(PermissionMapping.MapPermission).ToList()));
    }
}
