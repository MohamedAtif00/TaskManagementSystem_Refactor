using FluentValidation;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed class RemoveRolePermissionCommandValidator : AbstractValidator<RemoveRolePermissionCommand>
{
    public RemoveRolePermissionCommandValidator()
    {
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PermissionId).GreaterThan(0);
    }
}
