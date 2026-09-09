using FluentAssertions;
using TaskManagementSystem.Modules.Identity.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.Identity.UnitTests;

public sealed class LoginCodeTests
{
    [Fact]
    public void TryCreate_WhenCodeIsValid_ReturnsNormalizedLoginCode()
    {
        var result = LoginCode.TryCreate("  tst001  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("TST001");
    }

    [Fact]
    public void TryCreate_WhenCodeIsWrongLength_ReturnsFailure()
    {
        var result = LoginCode.TryCreate("ABC");

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("invalid_login_code");
    }
}
