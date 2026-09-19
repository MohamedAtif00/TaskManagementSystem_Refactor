using FluentValidation;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed class AddRolePermissionCommandValidator : AbstractValidator<AddRolePermissionCommand>
{
    public AddRolePermissionCommandValidator()
    {
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PermissionId).GreaterThan(0);
    }
}
