using FluentValidation;

namespace TaskManagementSystem.Modules.Notifications.Features.Notifications.ListMyNotifications;

public sealed class ListMyNotificationsQueryValidator : AbstractValidator<ListMyNotificationsQuery>
{
    public ListMyNotificationsQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

