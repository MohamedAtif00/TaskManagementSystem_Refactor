using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.GetPendingLeaveRequests;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class GetPendingLeaveRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapGet("/leave-requests/pending", HandleAsync)
            .RequireAuthorization(nameof(UserRole.Owner));
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPendingLeaveRequestsQuery(), cancellationToken);

        return result.ToHttpResult(leaveRequests =>
            Results.Ok(leaveRequests.Select(LeaveRequestMapping.MapLeaveRequest).ToList()));
    }
}
