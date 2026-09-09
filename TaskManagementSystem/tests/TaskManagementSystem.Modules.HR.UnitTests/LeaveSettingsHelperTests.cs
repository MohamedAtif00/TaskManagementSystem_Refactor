using FluentAssertions;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class LeaveSettingsHelperTests
{
    [Theory]
    [InlineData("04-30", 2026, 2026, 4, 30)]
    [InlineData("2026-05-01", 2026, 2026, 5, 1)]
    public void ParseDateForYear_ParsesMonthDayAndFullDate(
        string value,
        int year,
        int expectedYear,
        int expectedMonth,
        int expectedDay)
    {
        var parsed = LeaveSettingsHelper.ParseDateForYear(value, year);

        parsed.Should().NotBeNull();
        parsed!.Value.Year.Should().Be(expectedYear);
        parsed.Value.Month.Should().Be(expectedMonth);
        parsed.Value.Day.Should().Be(expectedDay);
    }

    [Fact]
    public void IsEmergencyAllowed_WhenBeforeCutoff_ReturnsTrue()
    {
        var settings = new LeaveSettingsOptions
        {
            EmergencyBlackoutCutoffDate = "04-30",
            ResetDate = "05-01"
        };

        LeaveSettingsHelper.IsEmergencyAllowed(settings, new DateTime(2026, 4, 1)).Should().BeTrue();
    }

    [Fact]
    public void IsInFromNextWindow_WhenInsideWindow_ReturnsTrue()
    {
        var settings = new LeaveSettingsOptions
        {
            FromNextBalanceStartDate = "01-01",
            FromNextBalanceEndDate = "04-30"
        };

        LeaveSettingsHelper.IsInFromNextWindow(settings, new DateTime(2026, 2, 1)).Should().BeTrue();
    }
}
