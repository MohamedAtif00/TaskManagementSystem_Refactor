namespace TaskManagementSystem.Modules.HR.Application;

public sealed class EmployeeBalance
{
    public int Id { get; init; }

    public int? TeamId { get; init; }

    public int? TeamleaderId { get; init; }

    public int AnnualLeave { get; set; }

    public int AnnualLeaveMax { get; init; }

    public int EmergencyLeave { get; set; }

    public int EmergencyLeaveMax { get; init; }

    public int SickLeave { get; set; }

    public int Permission { get; init; }

    public int PermissionMax { get; init; }

    public int WorkFromHome { get; init; }

    public int WorkFromHomeMax { get; init; }

    public int FromNextBalanceDaysUsed { get; set; }

    public int OldAnnualBalance { get; init; }

    public int AvailableAnnualLeave(int pendingWorkingDays) =>
        AnnualLeaveMax - AnnualLeave - pendingWorkingDays;

    public bool HasAvailableAnnualLeave(int requestedDays, int pendingWorkingDays) =>
        AvailableAnnualLeave(pendingWorkingDays) >= requestedDays;

    public int AvailableEmergencyLeave(int pendingWorkingDays) =>
        EmergencyLeaveMax - EmergencyLeave - pendingWorkingDays;

    public bool HasAvailableEmergencyLeave(int requestedDays, int pendingWorkingDays) =>
        AvailableEmergencyLeave(pendingWorkingDays) >= requestedDays;

    public int AvailableFromNextBalance(LeaveSettingsSnapshot settings, int pendingFromNextDays) =>
        settings.FromNextBalanceMaxDays - FromNextBalanceDaysUsed - pendingFromNextDays;

    public bool HasAvailableFromNextBalance(
        LeaveSettingsSnapshot settings,
        int requestedDays,
        int pendingFromNextDays) =>
        AvailableFromNextBalance(settings, pendingFromNextDays) >= requestedDays;

    public int AvailablePermission(int pendingPermissionCount) =>
        PermissionMax - Permission - pendingPermissionCount;

    public bool HasAvailablePermission(int pendingPermissionCount) =>
        AvailablePermission(pendingPermissionCount) >= 1;

    public int AvailableWorkFromHome(int pendingWorkFromHomeCount) =>
        WorkFromHomeMax - WorkFromHome - pendingWorkFromHomeCount;

    public bool HasAvailableWorkFromHome(int pendingWorkFromHomeCount) =>
        AvailableWorkFromHome(pendingWorkFromHomeCount) >= 1;
}

public sealed record LeaveSettingsSnapshot(
    int FromNextBalanceMaxDays,
    string? FromNextBalanceStartDate,
    string? FromNextBalanceEndDate,
    string? EmergencyBlackoutCutoffDate,
    string? ResetDate);
