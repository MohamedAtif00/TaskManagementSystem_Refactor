using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TaskManagementSystem.BuildingBlocks.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest);
        var requestName = MediatRRequestMetadata.GetRequestName(requestType);

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["TraceId"] = Activity.Current?.TraceId.ToString(),
            ["RequestKind"] = MediatRRequestMetadata.GetKind(requestType),
            ["ModuleName"] = MediatRRequestMetadata.GetModuleName(requestType),
            ["RequestName"] = requestName
        });

        logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next(cancellationToken);
            logger.LogInformation("Handled {RequestName}", requestName);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed {RequestName}", requestName);
            throw;
        }
    }
}
