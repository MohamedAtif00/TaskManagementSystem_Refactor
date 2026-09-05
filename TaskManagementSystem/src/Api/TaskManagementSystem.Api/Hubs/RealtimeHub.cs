using Microsoft.AspNetCore.SignalR;

namespace TaskManagementSystem.Api.Hubs;

/// <summary>
/// Single WebSocket endpoint for silent content sync and visible notifications.
/// Clients distinguish them via <c>RealtimeMessage.Kind</c>, not a second hub.
/// </summary>
public sealed class RealtimeHub : Hub
{
    public const string Path = "/realtime";

    public Task JoinGroup(string groupName) =>
        Groups.AddToGroupAsync(Context.ConnectionId, groupName);

    public Task LeaveGroup(string groupName) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
}
