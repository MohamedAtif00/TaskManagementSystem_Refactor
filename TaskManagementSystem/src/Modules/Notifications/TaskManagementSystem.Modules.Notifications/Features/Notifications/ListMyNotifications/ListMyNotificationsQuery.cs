using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Notifications.Application;

namespace TaskManagementSystem.Modules.Notifications.Features.Notifications.ListMyNotifications;

public sealed record ListMyNotificationsQuery(
    int UserId,
    bool? IsRead = null,
    int Page = 1,
    int PageSize = 20) : IQuery<Result<NotificationListPageResult>>;

