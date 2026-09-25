using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Permissions.SearchPermissionRequests;

namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

public static class SearchPermissionRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder permissions)
    {
        permissions.MapGet("/search", HandleAsync).RequirePermissionCode(PermissionCodes.HrTimeoff.Read);
        return permissions;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? date = null,
        string? fromDate = null,
        string? toDate = null,
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
                OptionalQueryBinding.ParseOptionalDate(date),
                OptionalQueryBinding.ParseOptionalDate(fromDate),
                OptionalQueryBinding.ParseOptionalDate(toDate),
                PermissionMapping.ParsePermissionStatus(status),
                PermissionMapping.ParsePermissionType(type),
                myStatus),
            cancellationToken);

        return result.ToHttpResult(list => Results.Ok(new PermissionRequestListResponse
        {
            Items = list.Items.Select(PermissionMapping.MapPermission).ToList(),
            Page = list.Page,
            PageSize = list.PageSize,
            TotalCount = list.TotalCount
        }));
    }
}
