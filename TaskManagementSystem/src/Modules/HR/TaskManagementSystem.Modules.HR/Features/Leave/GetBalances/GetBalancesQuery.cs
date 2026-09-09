using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Infrastructure;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GetBalances;

public sealed record GetBalancesQuery(int UserId) : IQuery<Result<BalancesResult>>;

public sealed record BalancesResult(
    int AnnualLeave,
    int AnnualLeaveMax,
    int AvailableAnnualLeave,
    int EmergencyLeave,
    int EmergencyLeaveMax,
    int AvailableEmergencyLeave,
    int SickLeave,
    int FromNextBalanceDaysUsed,
    int FromNextBalanceMaxDays,
    int Permission,
    int PermissionMax,
    int WorkFromHome,
    int WorkFromHomeMax);

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
            balance.WorkFromHome,
            balance.WorkFromHomeMax));
    }
}
