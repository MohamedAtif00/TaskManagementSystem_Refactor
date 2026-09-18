using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Events;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Outbox;

public sealed class EfOutboxWriter<TContext>(
    TContext context,
    IIntegrationEventSerializer serializer) : IOutboxWriter
    where TContext : DbContext
{
    public Task AddAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        context.Set<OutboxMessage>().Add(new OutboxMessage
        {
            Id = integrationEvent.Id,
            OccurredOnUtc = integrationEvent.OccurredOn,
            Type = integrationEvent.GetType().AssemblyQualifiedName!,
            Payload = serializer.Serialize(integrationEvent),
            Attempts = 0
        });

        return Task.CompletedTask;
    }
}
