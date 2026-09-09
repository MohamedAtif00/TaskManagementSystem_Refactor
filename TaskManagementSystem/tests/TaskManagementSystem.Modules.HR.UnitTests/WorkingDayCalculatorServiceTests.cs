using FluentAssertions;
using NSubstitute;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class WorkingDayCalculatorServiceTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CountAsync_WhenHolidayFallsOnWeekday_ExcludesHoliday()
    {
        var holiday = PublicHoliday.Create(
            "Public holiday",
            null,
            new DateTime(2027, 1, 5),
            new DateTime(2027, 1, 5),
            1,
            UtcNow).Value;

        var holidayRepository = Substitute.For<IHolidayRepository>();
        holidayRepository.GetInRangeAsync(
                new DateTime(2027, 1, 4),
                new DateTime(2027, 1, 6),
                Arg.Any<CancellationToken>())
            .Returns([holiday]);

        var calculator = new WorkingDayCalculatorService(holidayRepository);

        var count = await calculator.CountAsync(new DateTime(2027, 1, 4), new DateTime(2027, 1, 6));

        count.Should().Be(2);
    }

    [Fact]
    public async Task CountAsync_WhenMultiDayHoliday_ExcludesAllHolidayDays()
    {
        var holiday = PublicHoliday.Create(
            "Long break",
            null,
            new DateTime(2027, 1, 4),
            new DateTime(2027, 1, 6),
            1,
            UtcNow).Value;

        var holidayRepository = Substitute.For<IHolidayRepository>();
        holidayRepository.GetInRangeAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([holiday]);

        var calculator = new WorkingDayCalculatorService(holidayRepository);

        var count = await calculator.CountAsync(new DateTime(2027, 1, 4), new DateTime(2027, 1, 10));

        count.Should().Be(2);
    }
}

public sealed class PublicHolidayTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WhenEndDateIsBeforeStartDate_Fails()
    {
        var result = PublicHoliday.Create("Eid", null, new DateTime(2027, 1, 10), new DateTime(2027, 1, 5), 1, UtcNow);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("holiday_invalid_dates");
    }

    [Fact]
    public void Create_WhenNameIsEmpty_Fails()
    {
        var result = PublicHoliday.Create(" ", null, new DateTime(2027, 1, 5), new DateTime(2027, 1, 5), 1, UtcNow);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("holiday_invalid_name");
    }
}
