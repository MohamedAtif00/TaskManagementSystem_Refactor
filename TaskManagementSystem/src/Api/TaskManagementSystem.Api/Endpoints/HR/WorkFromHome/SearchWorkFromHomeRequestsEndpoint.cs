using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.SearchWorkFromHomeRequests;

namespace TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;

public static class SearchWorkFromHomeRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder workFromHome)
    {
        workFromHome.MapGet("/search", HandleAsync).RequirePermissionCode(PermissionCodes.HrWorkFromHome.Read);
        return workFromHome;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? status = null,
        string? myStatus = null,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var teamId = currentUser.GetTeamId();

        var result = await mediator.Send(
            new SearchWorkFromHomeRequestsQuery(
                userId,
                role,
                teamId,
                page,
                pageSize,
                search,
                fromDate,
                toDate,
                WorkFromHomeMapping.ParseWorkFromHomeStatus(status),
                myStatus),
            cancellationToken);

        return result.ToHttpResult(list => Results.Ok(new WorkFromHomeRequestListResponse
        {
            Items = list.Items.Select(WorkFromHomeMapping.MapWorkFromHome).ToList(),
            TotalCount = list.TotalCount
        }));
    }
}
