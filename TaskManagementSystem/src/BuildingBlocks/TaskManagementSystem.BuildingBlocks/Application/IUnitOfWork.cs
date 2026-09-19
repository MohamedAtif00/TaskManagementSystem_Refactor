namespace TaskManagementSystem.BuildingBlocks.Application;

public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);

    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
