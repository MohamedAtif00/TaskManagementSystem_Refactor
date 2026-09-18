using DbUp;
using DbUp.ScriptProviders;
using Microsoft.Data.SqlClient;
using TaskManagementSystem.Database;

if (args.Length == 3 && string.Equals(args[0], "--seed", StringComparison.OrdinalIgnoreCase))
{
    return await RunSeedAsync(args[1], args[2]);
}

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  DatabaseMigrator [connectionString] [pathToMigrationScripts]");
    Console.Error.WriteLine("  DatabaseMigrator --seed [connectionString] [pathToSeedScriptOrFolder]");
    return -1;
}

return RunMigrations(args[0], args[1]);

static int RunMigrations(string connectionString, string scriptsPath)
{
    if (!Directory.Exists(scriptsPath))
    {
        Console.Error.WriteLine($"Scripts directory not found: {scriptsPath}");
        return -1;
    }

    Console.WriteLine("Starting database migration...");

    EnsureAppSchemaExists(connectionString);

    var upgrader = DeployChanges.To
        .SqlDatabase(connectionString)
        .WithScriptsFromFileSystem(scriptsPath, new FileSystemScriptOptions
        {
            IncludeSubDirectories = true
        })
        .JournalToSqlTable("app", "MigrationsJournal")
        .LogToConsole()
        .Build();

    var result = upgrader.PerformUpgrade();

    if (!result.Successful)
    {
        Console.Error.WriteLine(result.Error);
        Console.Error.WriteLine("Migration failed.");
        return -1;
    }

    Console.WriteLine("Migration successful.");
    return 0;
}

static async Task<int> RunSeedAsync(string connectionString, string seedPath)
{
    if (!File.Exists(seedPath) && !Directory.Exists(seedPath))
    {
        Console.Error.WriteLine($"Seed path not found: {seedPath}");
        return -1;
    }

    Console.WriteLine("Starting database seed (not journaled)...");

    var seedFiles = File.Exists(seedPath)
        ? [seedPath]
        : Directory.GetFiles(seedPath, "*.sql")
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ToArray();

    foreach (var seedFile in seedFiles)
    {
        Console.WriteLine($"Running seed script: {Path.GetFileName(seedFile)}");
        await SqlScriptSeeder.ExecuteFileAsync(connectionString, seedFile);
    }

    Console.WriteLine("Seed successful.");
    return 0;
}

static void EnsureAppSchemaExists(string connectionString)
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();

    using var command = connection.CreateCommand();
    command.CommandText = """
        IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'app')
            EXEC(N'CREATE SCHEMA [app]');
        """;
    command.ExecuteNonQuery();
}
