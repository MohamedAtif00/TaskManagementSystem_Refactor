using MediatR;

namespace TaskManagementSystem.BuildingBlocks.Application.Behaviors;

public sealed class AuditBehavior<TRequest, TResponse>(
    IAuditContext auditContext,
    IAuditStore auditStore,
    TimeProvider timeProvider)
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
            success = true;
            return response;
        }
        finally
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
    }
}
