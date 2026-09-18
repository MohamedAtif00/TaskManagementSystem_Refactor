using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.SearchForgotClockRequests;

namespace TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

public static class SearchForgotClockRequestsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder forgotClock)
    {
        forgotClock.MapGet("/search", HandleAsync).RequirePermissionCode(PermissionCodes.HrForgotClock.Read);
        return forgotClock;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        DateTime? date = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? status = null,
        string? punchType = null,
        string? myStatus = null,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var teamId = currentUser.GetTeamId();

        var result = await mediator.Send(
            new SearchForgotClockRequestsQuery(
                userId,
                role,
                teamId,
                page,
                pageSize,
                search,
                date,
                fromDate,
                toDate,
                ForgotClockMapping.ParseForgotClockStatus(status),
                ForgotClockMapping.ParsePunchType(punchType),
                myStatus),
            cancellationToken);

        return result.ToHttpResult(list => Results.Ok(new ForgotClockRequestListResponse
        {
            Items = list.Items.Select(ForgotClockMapping.MapForgotClock).ToList(),
            TotalCount = list.TotalCount
        }));
    }
}
