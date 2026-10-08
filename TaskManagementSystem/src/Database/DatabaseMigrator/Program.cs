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
    try
    {
        DatabaseSchemaMigrator.Upgrade(connectionString, scriptsPath);
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        Console.Error.WriteLine("Migration failed.");
        return -1;
    }
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
