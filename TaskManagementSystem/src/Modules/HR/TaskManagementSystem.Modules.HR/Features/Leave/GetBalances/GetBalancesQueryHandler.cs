using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Infrastructure;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GetBalances;

public sealed class GetBalancesQueryHandler(
    IHrUnitOfWork unitOfWork,
    LeaveRequestPlanner planner)
    : IRequestHandler<GetBalancesQuery, Result<BalancesResult>>
{
    public async Task<Result<BalancesResult>> Handle(GetBalancesQuery request, CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.EmployeeBalances.GetByUserIdAsync(request.UserId, cancellationToken);
        if (balance is null)
        {
            return Result.Fail<BalancesResult>(HrErrors.UserNotFound);
        }

        var pendingByType = await unitOfWork.LeaveRequests.SumPendingWorkingDaysByTypeAsync(
            request.UserId,
            [Domain.LeaveType.Annual, Domain.LeaveType.Emergency, Domain.LeaveType.FromNextBalance],
            cancellationToken: cancellationToken);
        var pendingAnnual = pendingByType.GetValueOrDefault(Domain.LeaveType.Annual);
        var pendingEmergency = pendingByType.GetValueOrDefault(Domain.LeaveType.Emergency);
        var pendingFromNext = pendingByType.GetValueOrDefault(Domain.LeaveType.FromNextBalance);
        var pendingPermissions = await unitOfWork.PermissionRequests.CountPendingAsync(request.UserId, cancellationToken: cancellationToken);
        var pendingWorkFromHome = await unitOfWork.WorkFromHomeRequests.CountPendingAsync(request.UserId, cancellationToken: cancellationToken);

        return Result.Ok(new BalancesResult(
            balance.AnnualLeave,
            balance.AnnualLeaveMax,
            balance.AvailableAnnualLeave(pendingAnnual),
            balance.EmergencyLeave,
            balance.EmergencyLeaveMax,
            balance.AvailableEmergencyLeave(pendingEmergency),
            balance.SickLeave,
            balance.FromNextBalanceDaysUsed,
            planner.Settings.FromNextBalanceMaxDays,
            balance.Permission,
            balance.PermissionMax,
            balance.AvailablePermission(pendingPermissions),
            balance.WorkFromHome,
            balance.WorkFromHomeMax,
            balance.AvailableWorkFromHome(pendingWorkFromHome)));
    }
}

