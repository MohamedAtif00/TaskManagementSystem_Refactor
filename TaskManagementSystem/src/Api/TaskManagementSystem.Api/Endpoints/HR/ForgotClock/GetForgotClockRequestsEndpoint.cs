using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.GetForgotClockRequests;

namespace TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

public static class GetForgotClockRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder forgotClock)
    {
        forgotClock.MapGet("", HandleAsync).RequireAuthorization();
        return forgotClock;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new GetForgotClockRequestsQuery(userId), cancellationToken);

        return result.ToHttpResult(list =>
            Results.Ok(list.Select(ForgotClockMapping.MapForgotClock).ToList()));
    }
}
