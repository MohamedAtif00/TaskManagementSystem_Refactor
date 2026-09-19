using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Notifications.Application;

namespace TaskManagementSystem.Modules.Notifications.Features.Notifications.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler(INotificationsUnitOfWork unitOfWork)
    : IRequestHandler<MarkNotificationReadCommand, Result<NotificationDetailResult>>
{
    public async Task<Result<NotificationDetailResult>> Handle(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await unitOfWork.Notifications.GetByIdTrackedAsync(
            request.Id,
            request.UserId,
            cancellationToken);

        if (notification is null)
        {
            return Result.Fail<NotificationDetailResult>(NotificationsErrors.NotificationNotFound);
        }

        var markReadResult = notification.MarkRead();
        if (!markReadResult.IsSuccess)
        {
            return Result.Fail<NotificationDetailResult>(markReadResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(NotificationDetailResult.From(notification));
    }
}

