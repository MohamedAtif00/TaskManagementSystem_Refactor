using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;

namespace TaskManagementSystem.BuildingBlocks.Persistence;

public abstract class UnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    private readonly TContext _context;
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    protected UnitOfWork(TContext context, IDomainEventDispatcher? domainEventDispatcher = null)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_domainEventDispatcher is not null)
        {
            await _domainEventDispatcher.DispatchAsync(_context, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (_domainEventDispatcher is not null)
        {
            await _domainEventDispatcher.DispatchAsync(_context, cancellationToken);

            if (_context.ChangeTracker.HasChanges())
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new EfUnitOfWorkTransaction(transaction);
    }
}
