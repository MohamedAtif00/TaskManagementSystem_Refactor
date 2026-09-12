using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.BuildingBlocks.Application.Data;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Data;

public static class SqlConnectionFactoryExtensions
{
    public static IServiceCollection AddSqlConnectionFactory(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"ConnectionStrings:{connectionStringName} is not configured.");

        services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
        return services;
    }
}
