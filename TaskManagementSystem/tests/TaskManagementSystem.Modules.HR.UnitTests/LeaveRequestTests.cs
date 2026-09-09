using FluentAssertions;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class LeaveRequestTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateAnnual_WhenDatesAreValid_ReturnsPendingRequest()
    {
        var result = LeaveRequest.CreateAnnual(
            userId: 1,
            startDate: new DateTime(2027, 1, 4),
            endDate: new DateTime(2027, 1, 6),
            workingDays: 3,
            reason: "Vacation",
            noteForManager: "Out of office",
            teamleaderId: 2,
            sectionheadId: null,
            utcNow: UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(LeaveType.Annual);
        result.Value.Status.Should().Be(LeaveStatus.Pending);
        result.Value.WorkingDays.Should().Be(3);
    }

    [Fact]
    public void CreateAnnual_WhenStartDateIsInPast_ReturnsInvalidDatesError()
    {
        var result = LeaveRequest.CreateAnnual(
            userId: 1,
            startDate: new DateTime(2026, 9, 1),
            endDate: new DateTime(2026, 9, 3),
            workingDays: 2,
            reason: null,
            noteForManager: null,
            teamleaderId: null,
            sectionheadId: null,
            utcNow: UtcNow);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("leave_invalid_dates");
    }

    [Fact]
    public void Approve_WhenPending_SetsApprovedStatus()
    {
        var leaveRequest = CreatePendingLeaveRequest();

        var result = leaveRequest.Approve();

        result.IsSuccess.Should().BeTrue();
        leaveRequest.Status.Should().Be(LeaveStatus.Approved);
    }

    [Fact]
    public void Approve_WhenNotPending_ReturnsLeaveNotPendingError()
    {
        var leaveRequest = CreatePendingLeaveRequest();
        leaveRequest.Approve();

        var result = leaveRequest.Approve();

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("leave_not_pending");
    }

    [Fact]
    public void Cancel_WhenPending_SetsCancelledStatus()
    {
        var leaveRequest = CreatePendingLeaveRequest();

        var result = leaveRequest.Cancel(UtcNow);

        result.IsSuccess.Should().BeTrue();
        leaveRequest.Status.Should().Be(LeaveStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenApprovedAndFutureStart_SetsCancelledStatus()
    {
        var leaveRequest = CreatePendingLeaveRequest();
        leaveRequest.Approve();

        var result = leaveRequest.Cancel(UtcNow);

        result.IsSuccess.Should().BeTrue();
        leaveRequest.Status.Should().Be(LeaveStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenApprovedAndStarted_ReturnsCannotCancelError()
    {
        var leaveRequest = CreatePendingLeaveRequest(startDate: UtcNow.Date, workingDays: 1);
        leaveRequest.Approve();

        var result = leaveRequest.Cancel(UtcNow);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("leave_cannot_cancel");
    }

    private static LeaveRequest CreatePendingLeaveRequest(DateTime? startDate = null, int workingDays = 3)
    {
        var result = LeaveRequest.CreateAnnual(
            userId: 1,
            startDate: startDate ?? new DateTime(2027, 1, 4),
            endDate: startDate?.AddDays(2) ?? new DateTime(2027, 1, 6),
            workingDays: workingDays,
            reason: null,
            noteForManager: null,
            teamleaderId: null,
            sectionheadId: null,
            utcNow: UtcNow);

        return result.Value;
    }
}
