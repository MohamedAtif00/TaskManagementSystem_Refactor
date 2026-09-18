using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;

namespace TaskManagementSystem.TestCommon.Integration;

public static class IntegrationOutboxSupport
{
    private static readonly string[] OutboxSchemas = ["ticket", "identity"];

    public static async Task DrainAllAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var pump = scope.ServiceProvider.GetRequiredService<IOutboxPump>();

        foreach (var schema in OutboxSchemas)
        {
            while (await pump.ProcessBatchAsync(schema, cancellationToken))
            {
            }
        }
    }
}
