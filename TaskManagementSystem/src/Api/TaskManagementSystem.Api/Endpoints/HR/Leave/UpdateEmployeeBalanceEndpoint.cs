using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Leave.GetBalances;
using TaskManagementSystem.Modules.HR.Features.Leave.UpdateEmployeeBalance;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

public static class UpdateEmployeeBalanceEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder leave)
    {
        leave.MapPut("/balances/{userId:int}", HandleAsync).RequirePermissionCode(PermissionCodes.HrLeave.Manage);
        return leave;
    }

    private static async Task<IResult> HandleAsync(
        int userId,
        UpdateEmployeeBalanceRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var updated = await mediator.Send(
            new UpdateEmployeeBalanceCommand(
                userId,
                currentUser.GetRequiredRole(),
                request.AnnualLeave,
                request.AnnualLeaveMax,
                request.EmergencyLeave,
                request.EmergencyLeaveMax,
                request.SickLeave,
                request.Permission,
                request.PermissionMax,
                request.WorkFromHome,
                request.WorkFromHomeMax,
                request.FromNextBalanceDaysUsed),
            cancellationToken);
        if (!updated.IsSuccess)
        {
            return updated.ToHttpResult(_ => Results.Ok());
        }

        var balances = await mediator.Send(
            new GetBalancesQuery(userId, currentUser.GetRequiredUserId(), currentUser.GetRequiredRole()),
            cancellationToken);
        return balances.ToHttpResult(result => Results.Ok(LeaveBalancesMapping.Map(result)));
    }
}
