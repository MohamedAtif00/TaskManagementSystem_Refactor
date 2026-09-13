using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagementSystem.BuildingBlocks.Persistence.Data;
using TaskManagementSystem.Modules.Workflows.Application;
using TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence;
using TaskManagementSystem.Modules.Workflows.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Workflows.Infrastructure;

public static class WorkflowsModuleExtensions
{
    public static IServiceCollection AddWorkflowsModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSqlConnectionFactory(configuration);
        services.AddScoped<IOrganizationTeamLookup, OrganizationLookupQueries>();

        services.AddDbContext<WorkflowsDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IWorkflowsUnitOfWork, WorkflowsUnitOfWork>();

        return services;
    }
}
