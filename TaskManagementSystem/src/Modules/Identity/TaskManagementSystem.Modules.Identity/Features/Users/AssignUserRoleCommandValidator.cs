using FluentValidation;

namespace TaskManagementSystem.Modules.Identity.Features.Users;

public sealed class AssignUserRoleCommandValidator : AbstractValidator<AssignUserRoleCommand>
{
    public AssignUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
    }
}
