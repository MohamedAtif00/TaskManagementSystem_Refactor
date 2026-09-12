using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagementSystem.BuildingBlocks.Persistence.Data;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Queries;
using TaskManagementSystem.Modules.Identity.Infrastructure.Security;

namespace TaskManagementSystem.Modules.Identity.Infrastructure;

public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSqlConnectionFactory(configuration);
        services.AddScoped<AboutMeQueries>();

        services.AddDbContext<IdentityDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
