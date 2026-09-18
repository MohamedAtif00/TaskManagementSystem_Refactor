using TaskManagementSystem.Api.Contracts.Notifications;
using TaskManagementSystem.Modules.Notifications.Features;

namespace TaskManagementSystem.Api.Endpoints.Notifications;

internal static class NotificationMapping
{
    internal static NotificationListPageResponse MapNotificationListPage(NotificationListPageResult page) =>
        new()
        {
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
            Items = page.Items.Select(MapNotificationListItem).ToList()
        };

    internal static NotificationListItemResponse MapNotificationListItem(NotificationListItemResult notification) =>
        new()
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Category = notification.Category,
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            RelatedEntityId = notification.RelatedEntityId
        };

    internal static NotificationDetailResponse MapNotificationDetail(NotificationDetailResult notification) =>
        new()
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Category = notification.Category,
            Type = notification.Type,
            Status = notification.Status,
            IsRead = notification.IsRead,
            HasActions = notification.HasActions,
            AdditionalData = notification.AdditionalData,
            RelatedEntityId = notification.RelatedEntityId,
            CreatedAt = notification.CreatedAt,
            UserId = notification.UserId
        };
}
