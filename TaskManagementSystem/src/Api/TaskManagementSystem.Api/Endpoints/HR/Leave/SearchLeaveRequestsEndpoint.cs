using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave.SearchLeaveRequests;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class SearchLeaveRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapGet("/leave-requests/search", HandleAsync).RequireAuthorization();
        return leave;
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
        string? type = null,
        string? myStatus = null,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var teamId = currentUser.GetTeamId();

        LeaveStatus? parsedStatus = Enum.TryParse<LeaveStatus>(status, true, out var leaveStatus)
            ? leaveStatus
            : null;
        LeaveType? parsedType = LeaveRequestMapping.ParseLeaveType(type);

        var result = await mediator.Send(
            new SearchLeaveRequestsQuery(
                userId,
                role,
                teamId,
                page,
                pageSize,
                search,
                fromDate,
                toDate,
                parsedStatus,
                parsedType,
                myStatus),
            cancellationToken);

        return result.ToHttpResult(list => Results.Ok(new LeaveRequestListResponse
        {
            Items = list.Items.Select(LeaveRequestMapping.MapLeaveRequest).ToList(),
            TotalCount = list.TotalCount
        }));
    }
}
