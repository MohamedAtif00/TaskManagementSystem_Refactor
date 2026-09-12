using Microsoft.Data.SqlClient;
using TaskManagementSystem.Database;

namespace TaskManagementSystem.TestCommon.Integration;

public static class IntegrationTestDatabaseBootstrap
{
    private const string MigrationsRelativePath =
        "src/Database/TaskManagementSystem.Database/Scripts/Migrations";

    public static async Task InitializeAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseExistsAsync(connectionString, cancellationToken);

        var migrationsDirectory = ResolveMigrationsDirectory();
        var migrationFiles = Directory.GetFiles(migrationsDirectory, "*.sql")
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase);

        foreach (var migrationFile in migrationFiles)
        {
            await SqlScriptSeeder.ExecuteFileAsync(connectionString, migrationFile, cancellationToken);
        }
    }

    private static async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken cancellationToken)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Integration test connection string must include a database name.");
        }

        builder.InitialCatalog = "master";
        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF DB_ID(N'{databaseName}') IS NULL
            BEGIN
                CREATE DATABASE [{databaseName}];
            END
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string ResolveMigrationsDirectory()
    {
        var directory = AppContext.BaseDirectory;

        while (directory is not null)
        {
            var candidate = Path.Combine(directory, MigrationsRelativePath);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate migrations directory '{MigrationsRelativePath}' from '{AppContext.BaseDirectory}'.");
    }
}
