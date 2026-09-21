using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.GetBalances;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class GetBalancesByUserIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapGet("/balances/{userId:int}", HandleAsync).RequirePermissionCode(PermissionCodes.HrLeave.Read);
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        int userId,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var viewerUserId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(new GetBalancesQuery(userId, viewerUserId, role), cancellationToken);

        return result.ToHttpResult(balances => Results.Ok(LeaveBalancesMapping.Map(balances)));
    }
}
