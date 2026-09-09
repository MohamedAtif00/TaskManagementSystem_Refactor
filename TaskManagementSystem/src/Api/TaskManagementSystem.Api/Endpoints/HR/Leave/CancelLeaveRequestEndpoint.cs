using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.CancelLeaveRequest;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class CancelLeaveRequestEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapPut("/leave-requests/{id:int}/cancel", HandleAsync).RequireAuthorization();
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new CancelLeaveRequestCommand(userId, id), cancellationToken);

        return result.ToHttpResult(leaveRequest => Results.Ok(LeaveRequestMapping.MapLeaveRequest(leaveRequest)));
    }
}
