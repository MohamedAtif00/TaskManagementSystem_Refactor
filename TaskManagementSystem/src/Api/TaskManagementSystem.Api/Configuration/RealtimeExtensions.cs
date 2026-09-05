using TaskManagementSystem.Api.Hubs;
using TaskManagementSystem.Api.Realtime;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;

namespace TaskManagementSystem.Api.Configuration;

public static class RealtimeExtensions
{
    public static IServiceCollection AddRealtime(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IRealtimePublisher, SignalRRealtimePublisher>();
        return services;
    }

    public static WebApplication MapRealtimeHub(this WebApplication app)
    {
        app.MapHub<RealtimeHub>(RealtimeHub.Path);
        return app;
    }
}
