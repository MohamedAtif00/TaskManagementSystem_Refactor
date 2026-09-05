using DbUp;
using DbUp.ScriptProviders;
using Microsoft.Data.SqlClient;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: DatabaseMigrator [connectionString] [pathToScripts]");
    return -1;
}

var connectionString = args[0];
var scriptsPath = args[1];

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
