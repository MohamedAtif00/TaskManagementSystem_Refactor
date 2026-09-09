using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.GetLeaveRequests;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class GetLeaveRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapGet("/leave-requests", HandleAsync).RequireAuthorization();
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new GetLeaveRequestsQuery(userId), cancellationToken);

        return result.ToHttpResult(leaveRequests =>
            Results.Ok(leaveRequests.Select(LeaveRequestMapping.MapLeaveRequest).ToList()));
    }
}
