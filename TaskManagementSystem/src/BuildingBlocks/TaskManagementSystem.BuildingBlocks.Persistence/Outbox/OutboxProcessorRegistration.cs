using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Outbox;

public static class OutboxProcessorRegistration
{
    public static IServiceCollection AddOutboxProcessor(
        this IServiceCollection services,
        string schema,
        IHostEnvironment environment)
    {
        services.TryAddSingleton(_ => new OutboxOptions
        {
            PollInterval = environment.IsEnvironment("Testing")
                ? TimeSpan.FromMilliseconds(100)
                : TimeSpan.FromSeconds(2)
        });

        services.TryAddSingleton<IOutboxPump, OutboxPump>();

        services.AddHostedService(provider => new OutboxProcessor(
            schema,
            provider.GetRequiredService<IOutboxPump>(),
            provider.GetRequiredService<ILogger<OutboxProcessor>>(),
            provider.GetRequiredService<OutboxOptions>()));

        return services;
    }
}
