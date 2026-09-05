using FluentAssertions;
using FluentValidation.TestHelper;
using TaskManagementSystem.Modules.Identity.Features.Authenticate;
using Xunit;

namespace TaskManagementSystem.Modules.Identity.UnitTests;

public sealed class AuthenticateCommandValidatorTests
{
    private readonly AuthenticateCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCodeIsEmpty_HasValidationError()
    {
        var result = _validator.TestValidate(new AuthenticateCommand(string.Empty));
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_WhenCodeIsNotSixCharacters_HasValidationError()
    {
        var result = _validator.TestValidate(new AuthenticateCommand("ABC"));
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_WhenCodeIsValid_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(new AuthenticateCommand("TST001"));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
