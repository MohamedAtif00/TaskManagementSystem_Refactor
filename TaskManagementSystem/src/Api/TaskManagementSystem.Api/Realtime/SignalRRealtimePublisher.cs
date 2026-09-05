using Microsoft.AspNetCore.SignalR;
using TaskManagementSystem.Api.Hubs;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;

namespace TaskManagementSystem.Api.Realtime;

public sealed class SignalRRealtimePublisher : IRealtimePublisher
{
    private readonly IHubContext<RealtimeHub> _hubContext;

    public SignalRRealtimePublisher(IHubContext<RealtimeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishToUserAsync(
        string userId,
        RealtimeMessage message,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.User(userId).SendAsync(message.EventName, message, cancellationToken);
    }

    public Task PublishToGroupAsync(
        string groupName,
        RealtimeMessage message,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.Group(groupName).SendAsync(message.EventName, message, cancellationToken);
    }
}
