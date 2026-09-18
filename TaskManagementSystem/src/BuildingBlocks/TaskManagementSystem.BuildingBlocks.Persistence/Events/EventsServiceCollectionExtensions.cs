using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.BuildingBlocks.Application.Events;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.BuildingBlocks.Application.Outbox;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Events;

public static class EventsServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationEvents(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton<IIntegrationEventSerializer, IntegrationEventSerializer>();
        services.AddScoped<IIntegrationEventBus, InProcessIntegrationEventBus>();
        return services;
    }
}
