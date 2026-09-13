using FluentValidation;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.RequestPermission;

public sealed class RequestPermissionCommandValidator : AbstractValidator<RequestPermissionCommand>
{
    public RequestPermissionCommandValidator()
    {
        RuleFor(command => command.Type).IsInEnum();
        RuleFor(command => command.Reason).MaximumLength(1000);
        RuleFor(command => command.ToTime).GreaterThanOrEqualTo(command => command.FromTime);
    }
}
