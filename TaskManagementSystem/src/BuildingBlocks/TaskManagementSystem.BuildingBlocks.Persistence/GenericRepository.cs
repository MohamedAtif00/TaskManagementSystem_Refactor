using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace TaskManagementSystem.BuildingBlocks.Persistence;

public abstract class GenericRepository<TEntity, TContext>(TContext context)
    where TEntity : class
    where TContext : DbContext
{
    protected TContext Context { get; } = context;

    protected DbSet<TEntity> Set => Context.Set<TEntity>();

    protected Task<TEntity?> FindReadOnlyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    protected Task<TEntity?> FindReadOnlyAsync(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        query.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    protected Task<TEntity?> FindTrackedAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(predicate, cancellationToken);

    protected Task AddEntityAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        Set.AddAsync(entity, cancellationToken).AsTask();

    protected Task<bool> ExistsReadOnlyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        Set.AsNoTracking().AnyAsync(predicate, cancellationToken);
}
