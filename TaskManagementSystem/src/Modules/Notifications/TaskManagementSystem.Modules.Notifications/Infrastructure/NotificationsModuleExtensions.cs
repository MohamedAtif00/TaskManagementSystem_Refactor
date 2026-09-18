using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.BuildingBlocks.Persistence.Data;
using TaskManagementSystem.BuildingBlocks.Persistence.Inbox;
using TaskManagementSystem.Modules.Notifications.Application;
using TaskManagementSystem.Modules.Notifications.Infrastructure.Persistence;

namespace TaskManagementSystem.Modules.Notifications.Infrastructure;

public static class NotificationsModuleExtensions
{
    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSqlConnectionFactory(configuration);
        services.AddScoped<IInboxGuard, EfInboxGuard<NotificationsDbContext>>();

        services.AddDbContext<NotificationsDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<INotificationsUnitOfWork, NotificationsUnitOfWork>();

        return services;
    }
}
