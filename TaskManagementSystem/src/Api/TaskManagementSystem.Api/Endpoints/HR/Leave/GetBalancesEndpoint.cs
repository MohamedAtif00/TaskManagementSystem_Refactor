using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.GetBalances;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class GetBalancesEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapGet("/balances", HandleAsync).RequireAuthorization();
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var result = await mediator.Send(new GetBalancesQuery(userId), cancellationToken);

        return result.ToHttpResult(balances => Results.Ok(new LeaveBalancesResponse
        {
            AnnualLeave = balances.AnnualLeave,
            AnnualLeaveMax = balances.AnnualLeaveMax,
            AvailableAnnualLeave = balances.AvailableAnnualLeave,
            EmergencyLeave = balances.EmergencyLeave,
            EmergencyLeaveMax = balances.EmergencyLeaveMax,
            AvailableEmergencyLeave = balances.AvailableEmergencyLeave,
            SickLeave = balances.SickLeave,
            FromNextBalanceDaysUsed = balances.FromNextBalanceDaysUsed,
            FromNextBalanceMaxDays = balances.FromNextBalanceMaxDays,
            Permission = balances.Permission,
            PermissionMax = balances.PermissionMax,
            WorkFromHome = balances.WorkFromHome,
            WorkFromHomeMax = balances.WorkFromHomeMax
        }));
    }
}
