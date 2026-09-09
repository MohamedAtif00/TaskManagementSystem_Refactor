using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.GiveBulkLeaveOpinion;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class GiveBulkLeaveOpinionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapPost("/leave-requests/opinions/bulk", HandleAsync)
            .RequireAuthorization(nameof(UserRole.Owner));
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        GiveBulkLeaveOpinionRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(
            new GiveBulkLeaveOpinionCommand(
                userId,
                role,
                request.LeaveRequestIds,
                request.IsApproved,
                request.Comment),
            cancellationToken);

        return result.ToHttpResult(bulk => Results.Ok(bulk));
    }
}
