using FluentAssertions;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave.RequestLeave;
using Xunit;

namespace TaskManagementSystem.Modules.HR.UnitTests;

public sealed class RequestLeaveCommandValidatorTests
{
    [Fact]
    public void Validate_WhenEndDateIsBeforeStartDate_FailsValidation()
    {
        var validator = new RequestLeaveCommandValidator();

        var result = validator.Validate(new RequestLeaveCommand(
            1,
            "Member",
            LeaveType.Annual,
            new DateTime(2027, 1, 10),
            new DateTime(2027, 1, 5),
            null,
            null,
            false,
            false,
            null,
            null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RequestLeaveCommand.EndDate));
    }

    [Fact]
    public void Validate_WhenReasonExceedsMaxLength_FailsValidation()
    {
        var validator = new RequestLeaveCommandValidator();

        var result = validator.Validate(new RequestLeaveCommand(
            1,
            "Member",
            LeaveType.Annual,
            new DateTime(2027, 1, 4),
            new DateTime(2027, 1, 6),
            new string('x', 1001),
            null,
            false,
            false,
            null,
            null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RequestLeaveCommand.Reason));
    }
}
