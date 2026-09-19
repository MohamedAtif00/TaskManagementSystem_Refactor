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
    int AvailablePermission,
    int WorkFromHome,
    int WorkFromHomeMax,
    int AvailableWorkFromHome);
