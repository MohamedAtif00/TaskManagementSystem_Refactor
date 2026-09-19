using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Notifications.Application;

namespace TaskManagementSystem.Modules.Notifications.Features.Notifications.ListMyNotifications;

public sealed class ListMyNotificationsQueryHandler(INotificationsUnitOfWork unitOfWork)
    : IRequestHandler<ListMyNotificationsQuery, Result<NotificationListPageResult>>
{
    public async Task<Result<NotificationListPageResult>> Handle(
        ListMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await unitOfWork.Notifications.ListByUserAsync(
            request.UserId,
            request.IsRead,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Ok(new NotificationListPageResult(
            items.Select(NotificationListItemResult.From).ToList(),
            request.Page,
            request.PageSize,
            totalCount));
    }
}

