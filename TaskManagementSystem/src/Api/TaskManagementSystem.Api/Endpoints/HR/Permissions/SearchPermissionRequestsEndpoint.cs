using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Permissions.SearchPermissionRequests;

namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

public static class SearchPermissionRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder permissions)
    {
        permissions.MapGet("/search", HandleAsync).RequireAuthorization();
        return permissions;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        DateTime? date = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? status = null,
        string? type = null,
        string? myStatus = null,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var teamId = currentUser.GetTeamId();

        var result = await mediator.Send(
            new SearchPermissionRequestsQuery(
                userId,
                role,
                teamId,
                page,
                pageSize,
                search,
                date,
                fromDate,
                toDate,
                PermissionMapping.ParsePermissionStatus(status),
                PermissionMapping.ParsePermissionType(type),
                myStatus),
            cancellationToken);

        return result.ToHttpResult(list => Results.Ok(new PermissionRequestListResponse
        {
            Items = list.Items.Select(PermissionMapping.MapPermission).ToList(),
            TotalCount = list.TotalCount
        }));
    }
}
