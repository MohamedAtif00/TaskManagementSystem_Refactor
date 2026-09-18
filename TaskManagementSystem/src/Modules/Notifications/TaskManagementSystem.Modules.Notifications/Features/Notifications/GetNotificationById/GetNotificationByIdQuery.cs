using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Notifications.Application;

namespace TaskManagementSystem.Modules.Notifications.Features.Notifications.GetNotificationById;

public sealed record GetNotificationByIdQuery(int Id, int UserId) : IQuery<Result<NotificationDetailResult>>;

public sealed class GetNotificationByIdQueryValidator : AbstractValidator<GetNotificationByIdQuery>
{
    public GetNotificationByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

public sealed class GetNotificationByIdQueryHandler(INotificationsUnitOfWork unitOfWork)
    : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDetailResult>>
{
    public async Task<Result<NotificationDetailResult>> Handle(
        GetNotificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var notification = await unitOfWork.Notifications.GetByIdAsync(request.Id, request.UserId, cancellationToken);
        return notification is null
            ? Result.Fail<NotificationDetailResult>(NotificationsErrors.NotificationNotFound)
            : Result.Ok(NotificationDetailResult.From(notification));
    }
}
