using Microsoft.EntityFrameworkCore.Storage;
using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.BuildingBlocks.Persistence;

internal sealed class EfUnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        transaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default) =>
        transaction.RollbackAsync(cancellationToken);

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}
