using FluentValidation;

namespace TaskManagementSystem.Modules.Identity.Features.Users.ArchiveUser;

public sealed class ArchiveUserCommandValidator : AbstractValidator<ArchiveUserCommand>
{
    public ArchiveUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
