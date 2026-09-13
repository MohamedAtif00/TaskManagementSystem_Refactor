using FluentAssertions;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class PermissionRequestTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WhenValid_ReturnsPendingRequest()
    {
        var result = PermissionRequest.Create(
            1,
            PermissionType.EarlyDeparture,
            new DateTime(2027, 1, 5),
            new TimeOnly(9, 0),
            new TimeOnly(11, 0),
            "Appointment",
            2,
            null,
            UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(PermissionStatus.Pending);
        result.Value.Type.Should().Be(PermissionType.EarlyDeparture);
    }

    [Fact]
    public void Create_WhenToTimeBeforeFromTime_ReturnsInvalidTimesError()
    {
        var result = PermissionRequest.Create(
            1,
            PermissionType.EarlyDeparture,
            new DateTime(2027, 1, 5),
            new TimeOnly(11, 0),
            new TimeOnly(9, 0),
            null,
            null,
            null,
            UtcNow);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("permission_invalid_times");
    }

    [Fact]
    public void Cancel_WhenApprovedAndFutureDate_Succeeds()
    {
        var permission = PermissionRequest.Create(
            1,
            PermissionType.Departure,
            new DateTime(2027, 1, 10),
            new TimeOnly(14, 0),
            new TimeOnly(16, 0),
            null,
            null,
            null,
            UtcNow).Value;

        permission.Approve(UtcNow);
        var result = permission.Cancel(UtcNow, wasApproved: true);

        result.IsSuccess.Should().BeTrue();
        permission.Status.Should().Be(PermissionStatus.Cancelled);
    }
}
