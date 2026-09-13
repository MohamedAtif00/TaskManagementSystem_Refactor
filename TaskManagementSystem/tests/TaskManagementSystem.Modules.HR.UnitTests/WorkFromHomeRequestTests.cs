using FluentAssertions;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class WorkFromHomeRequestTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WhenDateIsValid_ReturnsPendingRequest()
    {
        var result = WorkFromHomeRequest.Create(
            1,
            new DateTime(2027, 1, 5),
            "Need focus time",
            2,
            null,
            UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(WorkFromHomeStatus.Pending);
    }

    [Fact]
    public void Create_WhenDateIsInPast_ReturnsInvalidDateError()
    {
        var result = WorkFromHomeRequest.Create(
            1,
            new DateTime(2026, 9, 1),
            null,
            null,
            null,
            UtcNow);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("work_from_home_invalid_date");
    }

    [Fact]
    public void Cancel_WhenApprovedAndFutureDate_Succeeds()
    {
        var request = WorkFromHomeRequest.Create(
            1,
            new DateTime(2027, 1, 10),
            null,
            null,
            null,
            UtcNow).Value;

        request.Approve();
        var result = request.Cancel(UtcNow);

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(WorkFromHomeStatus.Cancelled);
    }
}
