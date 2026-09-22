using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.ApproveLeaveRequest;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class ApproveLeaveRequestEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapPost("/leave-requests/{id:int}/approve", HandleAsync)
            .RequirePermissionCode(PermissionCodes.HrLeave.Manage);
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(new ApproveLeaveRequestCommand(userId, role, id), cancellationToken);

        return result.ToHttpResult(leaveRequest => Results.Ok(LeaveRequestMapping.MapLeaveRequest(leaveRequest)));
    }
}
