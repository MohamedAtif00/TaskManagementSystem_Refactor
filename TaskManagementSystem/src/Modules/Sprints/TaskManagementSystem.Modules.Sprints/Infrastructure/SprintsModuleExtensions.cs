using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagementSystem.BuildingBlocks.Persistence.Data;
using TaskManagementSystem.Modules.Sprints.Application;
using TaskManagementSystem.Modules.Sprints.Infrastructure.Persistence;
using TaskManagementSystem.Modules.Sprints.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Sprints.Infrastructure;

public static class SprintsModuleExtensions
{
    public static IServiceCollection AddSprintsModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSqlConnectionFactory(configuration);
        services.AddScoped<ILearningObjectiveLookup, LearningObjectiveLookupQueries>();

        services.AddDbContext<SprintsDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<ISprintsUnitOfWork, SprintsUnitOfWork>();

        return services;
    }
}
