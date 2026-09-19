using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxProcessor(
    string schema,
    IOutboxPump outboxPump,
    ILogger<OutboxProcessor> logger,
    OutboxOptions options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedAny = await outboxPump.ProcessBatchAsync(schema, stoppingToken);
                if (!processedAny)
                {
                    await Task.Delay(options.PollInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox processor for schema '{Schema}' failed.", schema);
                await Task.Delay(options.PollInterval, stoppingToken);
            }
        }
    }
}
