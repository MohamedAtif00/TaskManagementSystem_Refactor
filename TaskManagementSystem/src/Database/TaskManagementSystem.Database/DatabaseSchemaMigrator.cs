using System;
using System.IO;
using DbUp;
using DbUp.Engine;
using DbUp.ScriptProviders;
using Microsoft.Data.SqlClient;

namespace TaskManagementSystem.Database;

public static class DatabaseSchemaMigrator
{
    public static void Upgrade(string connectionString, string scriptsPath)
    {
        if (!Directory.Exists(scriptsPath))
        {
            DatabaseStartupLog.Write("Migration scripts were not found: " + scriptsPath);
            throw new DirectoryNotFoundException($"Migration scripts were not found: {scriptsPath}");
        }

        DatabaseStartupLog.Write("Starting database migration from " + scriptsPath);
        try
        {
            EnsureAppSchemaExists(connectionString);
        }
        catch (Exception ex)
        {
            DatabaseStartupLog.Write("Creating the app schema failed: " + ex);
            throw;
        }

        UpgradeEngine upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsFromFileSystem(scriptsPath, new FileSystemScriptOptions
            {
                IncludeSubDirectories = true
            })
            .JournalToSqlTable("app", "MigrationsJournal")
            .WithExecutionTimeout(TimeSpan.FromHours(2))
            .LogTo(new DatabaseStartupLog())
            .Build();

        DatabaseUpgradeResult result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            DatabaseStartupLog.Write("Database migration failed: " + result.Error);
            throw new InvalidOperationException("Database migration failed.", result.Error);
        }

        DatabaseStartupLog.Write("Migration successful.");
    }

    private static void EnsureAppSchemaExists(string connectionString)
    {
        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'app')
                EXEC(N'CREATE SCHEMA [app]');
            """;
        command.ExecuteNonQuery();
    }
}
