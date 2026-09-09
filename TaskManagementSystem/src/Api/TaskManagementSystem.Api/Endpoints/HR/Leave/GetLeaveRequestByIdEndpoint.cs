using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.GetLeaveRequestById;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class GetLeaveRequestByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapGet("/leave-requests/{id:int}", HandleAsync).RequireAuthorization();
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
        var result = await mediator.Send(new GetLeaveRequestByIdQuery(userId, role, id), cancellationToken);

        return result.ToHttpResult(leaveRequest => Results.Ok(LeaveRequestMapping.MapLeaveRequest(leaveRequest)));
    }
}
