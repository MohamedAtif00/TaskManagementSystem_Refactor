using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.ApproveForgotClockRequest;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

public static class ApproveForgotClockRequestEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder forgotClock)
    {
        forgotClock.MapPost("/{id:int}/approve", HandleAsync)
            .RequireAuthorization(nameof(UserRole.Owner));
        return forgotClock;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new ApproveForgotClockRequestCommand(userId, id), cancellationToken);

        return result.ToHttpResult(item => Results.Ok(ForgotClockMapping.MapForgotClock(item)));
    }
}
