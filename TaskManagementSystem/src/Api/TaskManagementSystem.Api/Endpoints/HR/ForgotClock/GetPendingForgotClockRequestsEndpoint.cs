using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.GetPendingForgotClockRequests;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

public static class GetPendingForgotClockRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder forgotClock)
    {
        forgotClock.MapGet("/pending", HandleAsync)
            .RequirePermissionCode(PermissionCodes.HrForgotClock.Read);
        return forgotClock;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPendingForgotClockRequestsQuery(), cancellationToken);

        return result.ToHttpResult(list =>
            Results.Ok(list.Select(ForgotClockMapping.MapForgotClock).ToList()));
    }
}
