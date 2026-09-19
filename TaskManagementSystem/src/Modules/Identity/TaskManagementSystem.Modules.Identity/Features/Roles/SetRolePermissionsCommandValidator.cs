using FluentValidation;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed class SetRolePermissionsCommandValidator : AbstractValidator<SetRolePermissionsCommand>
{
    public SetRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
    }
}
