using FluentValidation;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
    }
}
