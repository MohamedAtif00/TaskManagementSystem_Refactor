using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;

namespace TaskManagementSystem.Modules.Ticket.Contracts;

/// <summary>
/// Silent content-sync messages for ticket boards and open tickets.
/// Does not create inbox items or toasts — use the Notifications module for those.
/// </summary>
public static class TicketRealtime
{
    public const string Updated = "ticket.updated";

    public static RealtimeMessage SilentUpdate(object payload) =>
        new(Updated, RealtimeDeliveryKind.Silent, payload);
}
