namespace TaskManagementSystem.BuildingBlocks.Application.Inbox;

public interface IInboxGuard
{
    Task<bool> TryBeginAsync(
        Guid integrationEventId,
        string consumerName,
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
