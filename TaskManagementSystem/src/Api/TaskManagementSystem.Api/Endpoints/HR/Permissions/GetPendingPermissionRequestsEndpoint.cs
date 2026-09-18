using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.HR.Features.Permissions.GetPendingPermissionRequests;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

public static class GetPendingPermissionRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder permissions)
    {
        permissions.MapGet("/pending", HandleAsync)
            .RequirePermissionCode(PermissionCodes.HrTimeoff.Read);
        return permissions;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPendingPermissionRequestsQuery(), cancellationToken);

        return result.ToHttpResult(list =>
            Results.Ok(list.Select(PermissionMapping.MapPermission).ToList()));
    }
}
