namespace TaskManagementSystem.Api.Contracts.HR;

public sealed class LeaveBalancesResponse
{
    public int AnnualLeave { get; set; }

    public int AnnualLeaveMax { get; set; }

    public int AvailableAnnualLeave { get; set; }

    public int EmergencyLeave { get; set; }

    public int EmergencyLeaveMax { get; set; }

    public int AvailableEmergencyLeave { get; set; }

    public int SickLeave { get; set; }

    public int FromNextBalanceDaysUsed { get; set; }

    public int FromNextBalanceMaxDays { get; set; }

    public int Permission { get; set; }

    public int PermissionMax { get; set; }

    public int WorkFromHome { get; set; }

    public int WorkFromHomeMax { get; set; }
}
