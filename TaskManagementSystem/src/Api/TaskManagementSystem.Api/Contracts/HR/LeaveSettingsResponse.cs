namespace TaskManagementSystem.Api.Contracts.HR;

public sealed class LeaveSettingsResponse
{
    public int FromNextBalanceMaxDays { get; set; }

    public string? FromNextBalanceStartDate { get; set; }

    public string? FromNextBalanceEndDate { get; set; }

    public string? EmergencyBlackoutCutoffDate { get; set; }

    public string? ResetDate { get; set; }

    public bool EmergencyAllowedToday { get; set; }

    public bool FromNextWindowActiveToday { get; set; }
}
