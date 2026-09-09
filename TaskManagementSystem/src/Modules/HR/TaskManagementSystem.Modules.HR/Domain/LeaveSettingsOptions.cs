namespace TaskManagementSystem.Modules.HR.Domain;

public sealed class LeaveSettingsOptions
{
    public const string SectionName = "LeaveSettings";

    public int FromNextBalanceMaxDays { get; set; } = 3;

    public string? FromNextBalanceStartDate { get; set; }

    public string? FromNextBalanceEndDate { get; set; }

    public string? EmergencyBlackoutCutoffDate { get; set; }

    public string? ResetDate { get; set; }

    public string? RemoveOldAnnualLeavesDate { get; set; }

    public string MedicalCertificateRelativePath { get; set; } = "medical-certificates";
}
