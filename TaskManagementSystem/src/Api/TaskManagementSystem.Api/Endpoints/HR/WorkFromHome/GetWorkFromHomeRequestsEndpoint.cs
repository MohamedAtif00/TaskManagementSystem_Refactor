using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.GetWorkFromHomeRequests;

namespace TaskManagementSystem.Api.Endpoints.HR.WorkFromHome;

public static class GetWorkFromHomeRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder workFromHome)
    {
        workFromHome.MapGet("", HandleAsync).RequireAuthorization();
        return workFromHome;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new GetWorkFromHomeRequestsQuery(userId), cancellationToken);

        return result.ToHttpResult(list =>
            Results.Ok(list.Select(WorkFromHomeMapping.MapWorkFromHome).ToList()));
    }
}
