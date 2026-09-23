namespace TaskManagementSystem.Modules.Ticket.Application;

public static class TicketRoles
{
    public static bool IsOwnerOrProjectManager(string? role) =>
        role is "Owner" or "ProjectManger";

    public static bool IsMember(string? role) => role == "Member";
}
