using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxProcessor(
    string schema,
    IOutboxPump outboxPump,
    ILogger<OutboxProcessor> logger,
    TimeSpan? pollInterval = null) : BackgroundService
{
    private readonly TimeSpan _pollInterval = pollInterval ?? TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedAny = await outboxPump.ProcessBatchAsync(schema, stoppingToken);
                if (!processedAny)
                {
                    await Task.Delay(_pollInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox processor for schema '{Schema}' failed.", schema);
                await Task.Delay(_pollInterval, stoppingToken);
            }
        }
    }
}
