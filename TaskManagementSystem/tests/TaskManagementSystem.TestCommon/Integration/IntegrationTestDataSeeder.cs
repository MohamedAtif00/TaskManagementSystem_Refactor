using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.BuildingBlocks.Application.Data;

namespace TaskManagementSystem.TestCommon.Integration;

public static class IntegrationTestDataSeeder
{
    public const string TestUserCode = "TST001";
    public const string TestTeamName = "Integration Test Team";
    public const string TestUserName = "Integration Test User";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
        var connectionString = connectionFactory.GetConnectionString();

        var seedScript = TaskManagementSystem.Database.SqlScriptSeeder.ResolveScriptPath(
            TaskManagementSystem.Database.SqlScriptSeeder.IdentityIntegrationTestSeedScript);
        await TaskManagementSystem.Database.SqlScriptSeeder.ExecuteFileAsync(
            connectionString,
            seedScript,
            cancellationToken);
    }
}
