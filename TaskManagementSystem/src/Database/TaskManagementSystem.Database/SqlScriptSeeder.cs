using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace TaskManagementSystem.Database;

public static class SqlScriptSeeder
{
    public const string IdentityIntegrationTestSeedScript =
        "src/Database/TaskManagementSystem.Database/Scripts/Migrations/011_identity_SeedUsers.sql";

    public static string ResolveScriptPath(string relativePathFromSolutionRoot)
    {
        var directory = AppContext.BaseDirectory;

        while (directory is not null)
        {
            var candidate = Path.Combine(directory, relativePathFromSolutionRoot);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new FileNotFoundException(
            $"Could not locate SQL seed script '{relativePathFromSolutionRoot}' from '{AppContext.BaseDirectory}'.");
    }

    public static async Task ExecuteFileAsync(
        string connectionString,
        string scriptPath,
        CancellationToken cancellationToken = default)
    {
        var script = await File.ReadAllTextAsync(scriptPath, cancellationToken);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var batch in SplitIntoBatches(script))
        {
            if (string.IsNullOrWhiteSpace(batch))
            {
                continue;
            }

            await using var command = connection.CreateCommand();
            command.CommandText = batch;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public static IEnumerable<string> SplitIntoBatches(string script) =>
        script.Split(["\r\nGO\r\n", "\nGO\n", "\r\nGO\n", "\nGO\r\n"], StringSplitOptions.RemoveEmptyEntries);
}
