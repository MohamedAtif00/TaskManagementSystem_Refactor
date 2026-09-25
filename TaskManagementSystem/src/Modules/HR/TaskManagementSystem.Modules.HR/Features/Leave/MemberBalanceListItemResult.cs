namespace TaskManagementSystem.Modules.HR.Features.Leave;

public sealed record MemberBalanceListItemResult(
    int UserId,
    string Code,
    string Name,
    int AnnualLeave,
    int AnnualLeaveMax,
    int EmergencyLeave,
    int EmergencyLeaveMax,
    int SickLeave,
    int Permission,
    int PermissionMax,
    int WorkFromHome,
    int WorkFromHomeMax,
    int FromNextBalanceDaysUsed);
