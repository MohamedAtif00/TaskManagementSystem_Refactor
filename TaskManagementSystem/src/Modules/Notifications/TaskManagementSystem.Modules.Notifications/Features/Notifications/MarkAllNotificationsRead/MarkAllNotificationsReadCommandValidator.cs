using FluentValidation;

namespace TaskManagementSystem.Modules.Notifications.Features.Notifications.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandValidator : AbstractValidator<MarkAllNotificationsReadCommand>
{
    public MarkAllNotificationsReadCommandValidator() => RuleFor(x => x.UserId).GreaterThan(0);
}

