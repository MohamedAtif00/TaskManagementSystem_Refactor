using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Modules.HR.Features.Leave.GetBalances;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

internal static class LeaveBalancesMapping
{
    internal static LeaveBalancesResponse Map(BalancesResult balances) =>
        new()
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
            AvailablePermission = balances.AvailablePermission,
            WorkFromHome = balances.WorkFromHome,
            WorkFromHomeMax = balances.WorkFromHomeMax,
            AvailableWorkFromHome = balances.AvailableWorkFromHome
        };
}
