using AutomatedTaskSystem.Models.Configs;

namespace AutomatedTaskSystem.Test;

public class LeaveSettingsTests
{
    [Fact]
    public void ParseDateForYear_EmptyOrNull_ReturnsNull()
    {
        Assert.Null(LeaveSettings.ParseDateForYear(null, 2025));
        Assert.Null(LeaveSettings.ParseDateForYear("", 2025));
        Assert.Null(LeaveSettings.ParseDateForYear("   ", 2025));
    }

    [Fact]
    public void ParseDateForYear_MMdd_UsesGivenYear()
    {
        var d = LeaveSettings.ParseDateForYear("04-30", 2025);
        Assert.NotNull(d);
        Assert.Equal(2025, d.Value.Year);
        Assert.Equal(4, d.Value.Month);
        Assert.Equal(30, d.Value.Day);
    }

    [Fact]
    public void ParseDateForYear_FullDate_ReturnsAsIs()
    {
        var d = LeaveSettings.ParseDateForYear("2025-06-15", 2024);
        Assert.NotNull(d);
        Assert.Equal(2025, d.Value.Year);
        Assert.Equal(6, d.Value.Month);
        Assert.Equal(15, d.Value.Day);
    }

    [Fact]
    public void EmergencyAllowed_AfterCutoffBeforeReset_IsBlocked()
    {
        int year = 2025;
        var today = new DateTime(2025, 5, 15); // between 04-30 and 05-01 reset
        var cutoff = LeaveSettings.ParseDateForYear("04-30", year);
        var resetDate = LeaveSettings.ParseDateForYear("05-01", year);
        bool emergencyAllowed = (cutoff == null && resetDate == null) ||
            (cutoff != null && today <= cutoff.Value) ||
            (resetDate != null && today >= resetDate.Value);
        Assert.False(emergencyAllowed);
    }

    [Fact]
    public void EmergencyAllowed_OnOrBeforeCutoff_IsAllowed()
    {
        int year = 2025;
        var today = new DateTime(2025, 4, 30);
        var cutoff = LeaveSettings.ParseDateForYear("04-30", year);
        var resetDate = LeaveSettings.ParseDateForYear("05-01", year);
        bool emergencyAllowed = (cutoff == null && resetDate == null) ||
            (cutoff != null && today <= cutoff.Value) ||
            (resetDate != null && today >= resetDate.Value);
        Assert.True(emergencyAllowed);
    }

    [Fact]
    public void EmergencyAllowed_OnOrAfterResetDate_IsAllowed()
    {
        int year = 2025;
        var today = new DateTime(2025, 5, 1);
        var cutoff = LeaveSettings.ParseDateForYear("04-30", year);
        var resetDate = LeaveSettings.ParseDateForYear("05-01", year);
        bool emergencyAllowed = (cutoff == null && resetDate == null) ||
            (cutoff != null && today <= cutoff.Value) ||
            (resetDate != null && today >= resetDate.Value);
        Assert.True(emergencyAllowed);
    }
}
