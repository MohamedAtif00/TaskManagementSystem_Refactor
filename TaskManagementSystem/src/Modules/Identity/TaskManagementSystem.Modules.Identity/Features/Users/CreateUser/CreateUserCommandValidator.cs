using FluentValidation;

namespace TaskManagementSystem.Modules.Identity.Features.Users.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.HrCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Title).MaximumLength(100);
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
    }
}
