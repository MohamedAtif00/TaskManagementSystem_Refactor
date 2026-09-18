using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.CancelForgotClockRequest;

namespace TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

public static class CancelForgotClockRequestEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder forgotClock)
    {
        forgotClock.MapPut("/{id:int}/cancel", HandleAsync).RequirePermissionCode(PermissionCodes.HrForgotClock.Update);
        return forgotClock;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new CancelForgotClockRequestCommand(userId, id), cancellationToken);

        return result.ToHttpResult(item => Results.Ok(ForgotClockMapping.MapForgotClock(item)));
    }
}
