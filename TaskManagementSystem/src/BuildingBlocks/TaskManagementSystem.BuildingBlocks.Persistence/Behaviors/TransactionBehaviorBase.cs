using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Behaviors;

public abstract class TransactionBehaviorBase<TRequest, TResponse, TDbContext>(
    TDbContext context,
    ILogger logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TDbContext : DbContext
{
    protected abstract bool AppliesTo(TRequest request);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!AppliesTo(request))
        {
            return await next(cancellationToken);
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next(cancellationToken);

            if (response is IResult { IsSuccess: false })
            {
                await transaction.RollbackAsync(cancellationToken);
                return response;
            }

            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Rolling back transaction for {RequestType}.",
                typeof(TRequest).Name);

            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
