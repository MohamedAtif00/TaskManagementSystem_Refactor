using Microsoft.EntityFrameworkCore;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(DbContext context, CancellationToken cancellationToken = default);
}
