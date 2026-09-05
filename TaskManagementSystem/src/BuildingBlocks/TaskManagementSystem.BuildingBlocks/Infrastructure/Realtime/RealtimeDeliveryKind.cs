namespace TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;

/// <summary>
/// Distinguishes silent UI sync from user-visible alerts on the same hub.
/// </summary>
public enum RealtimeDeliveryKind
{
    /// <summary>
    /// Patch client state with no toast or inbox item (boards, lists, open records).
    /// </summary>
    Silent = 0,

    /// <summary>
    /// Something the user should notice (toast, badge, persisted inbox).
    /// Produced by the Notifications module, not by silent content sync.
    /// </summary>
    Notification = 1
}
