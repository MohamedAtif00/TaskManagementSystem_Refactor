using FluentAssertions;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class LeaveTypeMappingTests
{
    [Theory]
    [InlineData("Unpaid", LeaveType.UnpaidLeave)]
    [InlineData("unpaid", LeaveType.UnpaidLeave)]
    [InlineData("UnpaidLeave", LeaveType.UnpaidLeave)]
    [InlineData("Annual", LeaveType.Annual)]
    public void Parse_AcceptsLegacyAndCurrentValues(string input, LeaveType expected)
    {
        LeaveTypeMapping.Parse(input).Should().Be(expected);
    }

    [Fact]
    public void GetStorageValues_ForUnpaidLeave_IncludesLegacyValue()
    {
        LeaveTypeMapping.GetStorageValues(LeaveType.UnpaidLeave)
            .Should().BeEquivalentTo(["UnpaidLeave", "Unpaid"]);
    }
}
