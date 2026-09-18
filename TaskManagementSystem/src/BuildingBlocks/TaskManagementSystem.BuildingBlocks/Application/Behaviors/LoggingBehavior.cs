using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TaskManagementSystem.BuildingBlocks.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    IHostEnvironment? hostEnvironment = null)
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
        var requestKind = MediatRRequestMetadata.GetKind(requestType);
        var logQueryDuration = hostEnvironment?.IsDevelopment() == true
            && string.Equals(requestKind, "query", StringComparison.OrdinalIgnoreCase);

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["TraceId"] = Activity.Current?.TraceId.ToString(),
            ["RequestKind"] = requestKind,
            ["ModuleName"] = MediatRRequestMetadata.GetModuleName(requestType),
            ["RequestName"] = requestName
        });

        logger.LogInformation("Handling {RequestName}", requestName);

        var stopwatch = logQueryDuration ? Stopwatch.StartNew() : null;

        try
        {
            var response = await next(cancellationToken);

            if (logQueryDuration)
            {
                stopwatch!.Stop();
                logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogInformation("Handled {RequestName}", requestName);
            }

            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed {RequestName}", requestName);
            throw;
        }
    }
}
