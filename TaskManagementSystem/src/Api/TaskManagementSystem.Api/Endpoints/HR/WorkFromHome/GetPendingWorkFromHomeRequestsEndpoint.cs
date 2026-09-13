using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.GetPendingWorkFromHomeRequests;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;

public static class GetPendingWorkFromHomeRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder workFromHome)
    {
        workFromHome.MapGet("/pending", HandleAsync)
            .RequireAuthorization(nameof(UserRole.Owner));
        return workFromHome;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPendingWorkFromHomeRequestsQuery(), cancellationToken);

        return result.ToHttpResult(list =>
            Results.Ok(list.Select(WorkFromHomeMapping.MapWorkFromHome).ToList()));
    }
}
