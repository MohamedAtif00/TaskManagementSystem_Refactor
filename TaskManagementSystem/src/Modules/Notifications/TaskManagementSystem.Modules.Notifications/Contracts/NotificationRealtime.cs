using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;

namespace TaskManagementSystem.Modules.Notifications.Contracts;

/// <summary>
/// Visible alerts only (persisted inbox, toast, badge).
/// Silent UI patches belong on <see cref="IRealtimePublisher"/> with
/// <see cref="RealtimeDeliveryKind.Silent"/> from the owning domain module.
/// </summary>
public static class NotificationRealtime
{
    public const string Created = "notification.created";

    public static RealtimeMessage Visible(object payload) =>
        new(Created, RealtimeDeliveryKind.Notification, payload);
}
