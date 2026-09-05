namespace TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;

/// <summary>
/// Port for pushing messages over the host WebSocket. Domain modules depend on this
/// abstraction only — never on SignalR. Implementation lives in the API adapter.
/// </summary>
public interface IRealtimePublisher
{
    Task PublishToUserAsync(
        string userId,
        RealtimeMessage message,
        CancellationToken cancellationToken = default);

    Task PublishToGroupAsync(
        string groupName,
        RealtimeMessage message,
        CancellationToken cancellationToken = default);
}
