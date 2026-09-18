using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.GiveBulkForgotClockOpinion;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

public static class GiveBulkForgotClockOpinionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder forgotClock)
    {
        forgotClock.MapPost("/opinions/bulk", HandleAsync)
            .RequirePermissionCode(PermissionCodes.HrForgotClock.Manage);
        return forgotClock;
    }

    private static async Task<IResult> HandleAsync(
        GiveBulkForgotClockOpinionRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(
            new GiveBulkForgotClockOpinionCommand(
                userId,
                role,
                request.ForgotClockRequestIds,
                request.IsApproved,
                request.Comment),
            cancellationToken);

        return result.ToHttpResult(bulk => Results.Ok(new BulkForgotClockOpinionResponse
        {
            Succeeded = bulk.Succeeded,
            Failed = bulk.Failed,
            FailedForgotClockRequestIds = bulk.FailedForgotClockRequestIds.ToList()
        }));
    }
}
