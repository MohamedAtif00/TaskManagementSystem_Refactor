using MediatR;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.BuildingBlocks.Application.Behaviors;

public sealed class AuditBehavior<TRequest, TResponse>(
    IAuditContext auditContext,
    IAuditStore auditStore,
    TimeProvider timeProvider,
    ILogger<AuditBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!MediatRRequestMetadata.IsCommand(typeof(TRequest)))
        {
            return await next(cancellationToken);
        }

        var success = false;

        try
        {
            var response = await next(cancellationToken);
            success = response is IResult result ? result.IsSuccess : true;
            return response;
        }
        finally
        {
            try
            {
                await auditStore.AppendAsync(
                    new AuditEntry(
                        auditContext.CorrelationId,
                        auditContext.UserId,
                        typeof(TRequest).Name,
                        timeProvider.GetUtcNow().UtcDateTime,
                        success),
                    cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to append audit entry for {RequestType}.", typeof(TRequest).Name);
            }
        }
    }
}
