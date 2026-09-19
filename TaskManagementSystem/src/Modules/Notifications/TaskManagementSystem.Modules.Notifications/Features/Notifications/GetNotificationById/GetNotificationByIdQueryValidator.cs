using FluentValidation;

namespace TaskManagementSystem.Modules.Notifications.Features.Notifications.GetNotificationById;

public sealed class GetNotificationByIdQueryValidator : AbstractValidator<GetNotificationByIdQuery>
{
    public GetNotificationByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

