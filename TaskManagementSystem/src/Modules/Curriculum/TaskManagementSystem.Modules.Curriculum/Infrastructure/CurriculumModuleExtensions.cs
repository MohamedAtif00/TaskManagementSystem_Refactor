using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagementSystem.BuildingBlocks.Persistence.Data;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;
using TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure;

public static class CurriculumModuleExtensions
{
    public static IServiceCollection AddCurriculumModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSqlConnectionFactory(configuration);
        services.AddScoped<IWorkflowSchemaLookup, WorkflowSchemaLookupQueries>();
        services.AddScoped<IIdentityUserLookup, IdentityUserLookupQueries>();
        services.AddScoped<YearTreeQueries>();

        services.AddDbContext<CurriculumDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<ICurriculumUnitOfWork, CurriculumUnitOfWork>();

        return services;
    }
}
