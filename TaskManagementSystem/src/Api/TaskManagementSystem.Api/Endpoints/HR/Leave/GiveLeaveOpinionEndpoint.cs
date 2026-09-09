using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.GiveLeaveOpinion;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class GiveLeaveOpinionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapPost("/leave-requests/{id:int}/opinions", HandleAsync).RequireAuthorization();
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        GiveLeaveOpinionRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(
            new GiveLeaveOpinionCommand(userId, role, id, request.IsApproved, request.Comment),
            cancellationToken);

        return result.ToHttpResult(leaveRequest => Results.Ok(LeaveRequestMapping.MapLeaveRequest(leaveRequest)));
    }
}
