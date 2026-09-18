using TaskManagementSystem.Modules.Notifications.Domain;

namespace TaskManagementSystem.Modules.Notifications.Features;

public sealed record NotificationListItemResult(
    int Id,
    string Title,
    string Message,
    string Category,
    string Type,
    bool IsRead,
    DateTime CreatedAt,
    int? RelatedEntityId)
{
    public static NotificationListItemResult From(Notification notification) =>
        new(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Category,
            notification.Type,
            notification.IsRead,
            notification.CreatedAt,
            notification.RelatedEntityId);
}

public sealed record NotificationDetailResult(
    int Id,
    string Title,
    string Message,
    string Category,
    string Type,
    string? Status,
    bool IsRead,
    bool HasActions,
    string? AdditionalData,
    int? RelatedEntityId,
    DateTime CreatedAt,
    int UserId)
{
    public static NotificationDetailResult From(Notification notification) =>
        new(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Category,
            notification.Type,
            notification.Status,
            notification.IsRead,
            notification.HasActions,
            notification.AdditionalData,
            notification.RelatedEntityId,
            notification.CreatedAt,
            notification.UserId);
}

public sealed record NotificationListPageResult(
    IReadOnlyList<NotificationListItemResult> Items,
    int Page,
    int PageSize,
    int TotalCount);
