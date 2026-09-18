using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.BuildingBlocks.Application.Events;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxPump(
    ISqlConnectionFactory connectionFactory,
    IServiceScopeFactory scopeFactory,
    IIntegrationEventSerializer serializer,
    ILogger<OutboxPump> logger) : IOutboxPump
{
    private const int BatchSize = 20;

    public async Task<bool> ProcessBatchAsync(string schema, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.GetOpenConnection();

        var messages = (await connection.QueryAsync<OutboxMessage>(
            new CommandDefinition(
                $"""
                 SELECT TOP (@BatchSize)
                     [Id],
                     [OccurredOnUtc],
                     [Type],
                     [Payload],
                     [ProcessedOnUtc],
                     [Error],
                     [Attempts]
                 FROM [{schema}].[OutboxMessages]
                 WHERE [ProcessedOnUtc] IS NULL
                 ORDER BY [OccurredOnUtc]
                 """,
                new { BatchSize },
                cancellationToken: cancellationToken))).ToList();

        if (messages.Count == 0)
        {
            return false;
        }

        foreach (var message in messages)
        {
            try
            {
                var integrationEvent = serializer.Deserialize(message.Type, message.Payload);

                using var scope = scopeFactory.CreateScope();
                var bus = scope.ServiceProvider.GetRequiredService<IIntegrationEventBus>();
                await bus.PublishAsync(integrationEvent, cancellationToken);

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        $"""
                         UPDATE [{schema}].[OutboxMessages]
                         SET [ProcessedOnUtc] = SYSUTCDATETIME(),
                             [Error] = NULL
                         WHERE [Id] = @Id
                         """,
                        new { message.Id },
                        cancellationToken: cancellationToken));
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Failed to process outbox message {OutboxMessageId} in schema '{Schema}'.",
                    message.Id,
                    schema);

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        $"""
                         UPDATE [{schema}].[OutboxMessages]
                         SET [Attempts] = [Attempts] + 1,
                             [Error] = @Error
                         WHERE [Id] = @Id
                         """,
                        new
                        {
                            message.Id,
                            Error = exception.Message.Length > 4000
                                ? exception.Message[..4000]
                                : exception.Message
                        },
                        cancellationToken: cancellationToken));
            }
        }

        return true;
    }
}
