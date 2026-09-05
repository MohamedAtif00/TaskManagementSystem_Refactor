using FluentValidation;

namespace TaskManagementSystem.Modules.Identity.Features.Authenticate;

public sealed class AuthenticateCommandValidator : AbstractValidator<AuthenticateCommand>
{
    public AuthenticateCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6);
    }
}
