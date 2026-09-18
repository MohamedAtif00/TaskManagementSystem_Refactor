using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Notifications.Application;

public static class NotificationsErrors
{
    public static ResultError NotificationNotFound =>
        new("notification_not_found", "Notification not found.");
}
