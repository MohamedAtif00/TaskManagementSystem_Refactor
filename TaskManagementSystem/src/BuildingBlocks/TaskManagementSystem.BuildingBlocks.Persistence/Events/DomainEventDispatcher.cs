using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Events;

public sealed class DomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    public async Task DispatchAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var entities = context.ChangeTracker
                .Entries<Entity>()
                .Where(entry => entry.Entity.DomainEvents.Count > 0)
                .Select(entry => entry.Entity)
                .ToList();

            if (entities.Count == 0)
            {
                return;
            }

            var domainEvents = entities
                .SelectMany(entity =>
                {
                    var events = entity.DomainEvents.ToList();
                    entity.ClearDomainEvents();
                    return events;
                })
                .ToList();

            foreach (var domainEvent in domainEvents)
            {
                await publisher.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
