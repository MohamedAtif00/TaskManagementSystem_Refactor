using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Events;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Inbox;

public sealed class EfInboxGuard<TContext>(
    TContext context,
    IIntegrationEventSerializer serializer) : IInboxGuard
    where TContext : DbContext
{
    public async Task<bool> TryBeginAsync(
        Guid integrationEventId,
        string consumerName,
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var alreadyProcessed = await context.Set<InboxMessage>()
            .AnyAsync(
                message => message.IntegrationEventId == integrationEventId
                    && message.ConsumerName == consumerName,
                cancellationToken);

        if (alreadyProcessed)
        {
            return false;
        }

        context.Set<InboxMessage>().Add(new InboxMessage
        {
            Id = Guid.NewGuid(),
            IntegrationEventId = integrationEventId,
            ConsumerName = consumerName,
            OccurredOnUtc = integrationEvent.OccurredOn,
            Type = integrationEvent.GetType().AssemblyQualifiedName!,
            Payload = serializer.Serialize(integrationEvent),
            Attempts = 0
        });

        return true;
    }
}
