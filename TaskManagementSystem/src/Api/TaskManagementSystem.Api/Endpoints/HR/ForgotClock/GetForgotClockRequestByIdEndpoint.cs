using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.GetForgotClockRequestById;

namespace TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

public static class GetForgotClockRequestByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder forgotClock)
    {
        forgotClock.MapGet("/{id:int}", HandleAsync).RequireAuthorization();
        return forgotClock;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(new GetForgotClockRequestByIdQuery(userId, role, id), cancellationToken);

        return result.ToHttpResult(item => Results.Ok(ForgotClockMapping.MapForgotClock(item)));
    }
}
