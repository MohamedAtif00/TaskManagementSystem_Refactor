using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagementSystem.BuildingBlocks.Persistence.Data;
using TaskManagementSystem.Modules.Analytics.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Analytics.Infrastructure;

public static class AnalyticsModuleExtensions
{
    public static IServiceCollection AddAnalyticsModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSqlConnectionFactory(configuration);
        services.AddScoped<SubjectOverviewQueries>();
        services.AddScoped<SprintOverviewQueries>();

        return services;
    }
}
