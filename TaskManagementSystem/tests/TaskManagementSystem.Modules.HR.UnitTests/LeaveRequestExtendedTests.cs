using FluentAssertions;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class LeaveRequestExtendedTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateEmergency_WhenDatesAreValid_ReturnsPendingRequest()
    {
        var result = LeaveRequest.CreateEmergency(
            1,
            new DateTime(2027, 1, 4),
            new DateTime(2027, 1, 4),
            1,
            "Emergency",
            null,
            null,
            null,
            UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(LeaveType.Emergency);
    }

    [Fact]
    public void CreateSick_WhenMoreThanThreeDaysWithoutCertificate_Fails()
    {
        var result = LeaveRequest.CreateSick(
            1,
            new DateTime(2027, 1, 4),
            new DateTime(2027, 1, 8),
            4,
            null,
            null,
            null,
            null,
            UtcNow,
            hasMedicalCertificate: false);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("leave_medical_required");
    }

    [Fact]
    public void Reject_WhenPending_SetsRejectedStatus()
    {
        var leaveRequest = LeaveRequest.CreateAnnual(
            1,
            new DateTime(2027, 1, 4),
            new DateTime(2027, 1, 6),
            3,
            null,
            null,
            null,
            null,
            UtcNow).Value;

        var result = leaveRequest.Reject();

        result.IsSuccess.Should().BeTrue();
        leaveRequest.Status.Should().Be(LeaveStatus.Rejected);
    }

    [Fact]
    public void Opinion_Create_ReturnsOpinionWithValues()
    {
        var result = Opinion.Create(5, 2, true, "Looks good", UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.LeaveRequestId.Should().Be(5);
        result.Value.UserId.Should().Be(2);
        result.Value.IsApproved.Should().BeTrue();
    }
}
