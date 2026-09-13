using FluentAssertions;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class ForgotClockRequestTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WhenDateIsPast_ReturnsPendingRequest()
    {
        var result = ForgotClockRequest.Create(
            1,
            ForgotClockPunchType.ClockIn,
            new DateTime(2026, 9, 5),
            new TimeOnly(9, 0),
            "Forgot to punch",
            2,
            null,
            UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ForgotClockStatus.Pending);
    }

    [Fact]
    public void Create_WhenDateIsInFuture_ReturnsInvalidDateError()
    {
        var result = ForgotClockRequest.Create(
            1,
            ForgotClockPunchType.ClockOut,
            new DateTime(2026, 9, 10),
            new TimeOnly(17, 0),
            null,
            null,
            null,
            UtcNow);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("forgot_clock_invalid_date");
    }

    [Fact]
    public void Cancel_WhenPending_Succeeds()
    {
        var request = ForgotClockRequest.Create(
            1,
            ForgotClockPunchType.ClockIn,
            new DateTime(2026, 9, 5),
            new TimeOnly(9, 0),
            null,
            null,
            null,
            UtcNow).Value;

        var result = request.Cancel(UtcNow);

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ForgotClockStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenApprovedForPastDate_Fails()
    {
        var request = ForgotClockRequest.Create(
            1,
            ForgotClockPunchType.ClockOut,
            new DateTime(2026, 9, 5),
            new TimeOnly(17, 0),
            null,
            null,
            null,
            UtcNow).Value;

        request.Approve(UtcNow);
        var result = request.Cancel(UtcNow);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("forgot_clock_cannot_cancel");
    }

    [Fact]
    public void Cancel_WhenApprovedForToday_Succeeds()
    {
        var request = ForgotClockRequest.Create(
            1,
            ForgotClockPunchType.ClockIn,
            UtcNow.Date,
            new TimeOnly(9, 0),
            null,
            null,
            null,
            UtcNow).Value;

        request.Approve(UtcNow);
        var result = request.Cancel(UtcNow);

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ForgotClockStatus.Cancelled);
    }
}
