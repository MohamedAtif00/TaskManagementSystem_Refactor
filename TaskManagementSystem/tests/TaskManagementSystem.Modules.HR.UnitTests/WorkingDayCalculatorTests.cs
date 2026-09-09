using FluentAssertions;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class WorkingDayCalculatorTests
{
    [Fact]
    public void Count_WhenRangeIncludesFridayAndSaturday_ExcludesWeekendDays()
    {
        // Monday 2027-01-04 through Sunday 2027-01-10
        var count = WorkingDayCalculator.Count(
            new DateTime(2027, 1, 4),
            new DateTime(2027, 1, 10));

        count.Should().Be(5);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, true)]
    [InlineData(DayOfWeek.Tuesday, true)]
    [InlineData(DayOfWeek.Wednesday, true)]
    [InlineData(DayOfWeek.Thursday, true)]
    [InlineData(DayOfWeek.Friday, false)]
    [InlineData(DayOfWeek.Saturday, false)]
    [InlineData(DayOfWeek.Sunday, true)]
    public void IsWorkingDay_MatchesFridaySaturdayWeekendRule(DayOfWeek dayOfWeek, bool expected)
    {
        var date = NextDateOn(dayOfWeek);
        WorkingDayCalculator.IsWorkingDay(date).Should().Be(expected);
    }

    private static DateTime NextDateOn(DayOfWeek dayOfWeek)
    {
        var date = DateTime.UtcNow.Date;
        while (date.DayOfWeek != dayOfWeek)
        {
            date = date.AddDays(1);
        }

        return date;
    }
}
