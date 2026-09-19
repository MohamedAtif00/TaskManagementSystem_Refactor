using FluentAssertions;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class ForgotClockPunchTypeMappingTests
{
    [Theory]
    [InlineData("In", ForgotClockPunchType.ClockIn)]
    [InlineData("in", ForgotClockPunchType.ClockIn)]
    [InlineData("ClockIn", ForgotClockPunchType.ClockIn)]
    [InlineData("Out", ForgotClockPunchType.ClockOut)]
    [InlineData("out", ForgotClockPunchType.ClockOut)]
    [InlineData("ClockOut", ForgotClockPunchType.ClockOut)]
    public void Parse_AcceptsLegacyAndCurrentValues(string input, ForgotClockPunchType expected)
    {
        ForgotClockPunchTypeMapping.Parse(input).Should().Be(expected);
    }

    [Fact]
    public void GetStorageValues_ForClockOut_IncludesLegacyValue()
    {
        ForgotClockPunchTypeMapping.GetStorageValues(ForgotClockPunchType.ClockOut)
            .Should().BeEquivalentTo(["ClockOut", "Out"]);
    }
}
