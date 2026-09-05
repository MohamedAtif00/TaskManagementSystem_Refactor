using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.BuildingBlocks.Application.Behaviors;

namespace TaskManagementSystem.BuildingBlocks.Application;

public static class BuildingBlocksServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocks(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddMediatR(configuration =>
        {
            foreach (var assembly in assemblies)
            {
                configuration.RegisterServicesFromAssembly(assembly);
            }
        });

        services.AddValidatorsFromAssemblies(assemblies);
        return services;
    }
}
