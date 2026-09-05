using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.BuildingBlocks.Persistence;

public abstract class UnitOfWork<TContext>(TContext context) : IUnitOfWork
    where TContext : DbContext
{
    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
