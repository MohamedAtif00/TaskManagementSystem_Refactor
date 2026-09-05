using FluentAssertions;
using TaskManagementSystem.BuildingBlocks.Domain;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class BusinessRuleValidationExceptionTests
{
    [Fact]
    public void Constructor_SetsMessageFromRule()
    {
        var rule = new BrokenRule("Annual leave balance exceeded.");

        var exception = new BusinessRuleValidationException(rule);

        exception.Message.Should().Be("Annual leave balance exceeded.");
        exception.BrokenRule.Should().BeSameAs(rule);
    }

    private sealed class BrokenRule(string message) : IBusinessRule
    {
        public bool IsBroken() => true;

        public string Message { get; } = message;
    }
}
