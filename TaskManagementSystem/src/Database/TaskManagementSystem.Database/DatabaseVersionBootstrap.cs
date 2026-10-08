using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace TaskManagementSystem.Database;

public static class DatabaseVersionBootstrap
{
    public const string NamePrefix = "SystemAdminDB_Test_v";
    private const string VersionLock = "SystemAdminDB_Test_version_bump";

    private static readonly Regex VersionName = new(
        "^" + NamePrefix + @"(\d+)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SetVarLine = new(
        @"^\s*:setvar\b.*$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string? PromoteIfVersioned(string connectionString, string scriptsRoot)
    {
        SqlConnectionStringBuilder configured = new SqlConnectionStringBuilder(connectionString);
        if (!VersionName.IsMatch(configured.InitialCatalog))
        {
            return null;
        }

        SqlConnectionStringBuilder master = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        DatabaseStartupLog.Write("Connecting to " + master.DataSource + ", database master.");
        using SqlConnection connection = new SqlConnection(master.ConnectionString);
        try
        {
            connection.Open();
        }
        catch (Exception ex)
        {
            DatabaseStartupLog.Write("Connection to master failed: " + ex.Message);
            throw;
        }

        DatabaseStartupLog.Write("Connected. Waiting for the version lock.");
        AcquireVersionLock(connection);
        DatabaseStartupLog.Write("Version lock acquired.");
        try
        {
            string? remembered = ReadCurrentCatalog();
            if (remembered is not null && DatabaseExists(connection, remembered))
            {
                DatabaseStartupLog.Write(
                    "Using existing promoted database " + remembered + ". Not creating another database.");
                return WithCatalog(connectionString, remembered);
            }

            if (remembered is not null)
            {
                DatabaseStartupLog.Write(
                    "Promoted database " + remembered + " is not on this server. Creating a new copy.");
            }

            string migrationsPath = Path.Combine(scriptsRoot, "Migrations");
            string importPath = Path.Combine(scriptsRoot, "DataMigration", "001_ImportLegacy.sql");
            if (!Directory.Exists(migrationsPath))
            {
                DatabaseStartupLog.Write("Migration scripts were not found: " + migrationsPath);
                throw new DirectoryNotFoundException($"Migration scripts were not found: {migrationsPath}");
            }

            if (!File.Exists(importPath))
            {
                DatabaseStartupLog.Write("Legacy import script was not found: " + importPath);
                throw new FileNotFoundException("Legacy import script was not found.", importPath);
            }

            string sourceName = configured.InitialCatalog;
            if (!DatabaseExists(connection, sourceName))
            {
                DatabaseStartupLog.Write("Source database " + sourceName + " was not found.");
                throw new InvalidOperationException("Source database " + sourceName + " was not found.");
            }

            List<int> versions = ListVersionNumbers(connection);
            int maxNumber = VersionNumber(sourceName);
            for (int index = 0; index < versions.Count; index++)
            {
                if (versions[index] > maxNumber)
                {
                    maxNumber = versions[index];
                }
            }

            if (maxNumber == int.MaxValue)
            {
                throw new InvalidOperationException("The database version number cannot increase past int.MaxValue.");
            }

            string targetName = NamePrefix + (maxNumber + 1).ToString(CultureInfo.InvariantCulture);
            string targetConnection = WithCatalog(connectionString, targetName);
            bool legacy = IsLegacy(connection, sourceName);
            DatabaseStartupLog.Write(
                "Database versioning on " + master.DataSource + ": " + sourceName + " -> " + targetName
                + (legacy ? " (legacy import)." : " (backup and restore)."));

            if (legacy)
            {
                RunStep("Create " + targetName, () => CreateDatabase(connection, targetName));
                RunStep("Migrate " + targetName, () => DatabaseSchemaMigrator.Upgrade(targetConnection, migrationsPath));
                RunStep("Import " + sourceName + " into " + targetName, () => ImportLegacy(targetConnection, importPath, sourceName, targetName));
            }
            else
            {
                RunStep("Copy " + sourceName + " to " + targetName, () => CloneDatabase(connection, sourceName, targetName));
                RunStep("Migrate " + targetName, () => DatabaseSchemaMigrator.Upgrade(targetConnection, migrationsPath));
            }

            WriteCurrentCatalog(targetName);
            DatabaseStartupLog.Write("Database version created: " + targetName + " (copied from " + sourceName + ").");
            return targetConnection;
        }
        finally
        {
            ReleaseVersionLock(connection);
        }
    }

    private static void AcquireVersionLock(SqlConnection connection)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandTimeout = 0;
        command.CommandText = """
            DECLARE @result int;
            EXEC @result = sp_getapplock
                @Resource = @resource,
                @LockMode = N'Exclusive',
                @LockOwner = N'Session',
                @LockTimeout = -1;
            SELECT @result;
            """;
        command.Parameters.AddWithValue("@resource", VersionLock);
        int result = (int)command.ExecuteScalar()!;
        if (result < 0)
        {
            throw new InvalidOperationException($"Could not lock database versioning (sp_getapplock returned {result}).");
        }
    }

    private static void ReleaseVersionLock(SqlConnection connection)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            EXEC sp_releaseapplock
                @Resource = @resource,
                @LockOwner = N'Session';
            """;
        command.Parameters.AddWithValue("@resource", VersionLock);
        command.ExecuteNonQuery();
    }

    private static string CurrentCatalogPath() =>
        Path.Combine(AppContext.BaseDirectory, "logs", "database-current.txt");

    private static string? ReadCurrentCatalog()
    {
        string path = CurrentCatalogPath();
        if (!File.Exists(path))
        {
            return null;
        }

        string text = File.ReadAllText(path).Trim();
        if (!VersionName.IsMatch(text))
        {
            DatabaseStartupLog.Write(
                "Ignoring " + path + " because it does not name a SystemAdminDB_Test_vN database.");
            return null;
        }

        return text;
    }

    private static void WriteCurrentCatalog(string catalog)
    {
        string path = CurrentCatalogPath();
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, catalog + Environment.NewLine);
        DatabaseStartupLog.Write("Remembered promoted database in " + path);
    }

    private static int VersionNumber(string databaseName)
    {
        Match match = VersionName.Match(databaseName);
        if (!match.Success || !int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int number))
        {
            throw new InvalidOperationException("Database name '" + databaseName + "' is not SystemAdminDB_Test_vN.");
        }

        return number;
    }

    private static bool DatabaseExists(SqlConnection connection, string databaseName)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT CASE WHEN DB_ID(@name) IS NULL THEN 0 ELSE 1 END";
        command.Parameters.AddWithValue("@name", databaseName);
        return (int)command.ExecuteScalar()! == 1;
    }

    private static List<int> ListVersionNumbers(SqlConnection connection)
    {
        List<int> numbers = new List<int>();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT [name] FROM sys.databases";
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string name = reader.GetString(0);
            Match match = VersionName.Match(name);
            if (match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int number))
            {
                numbers.Add(number);
            }
        }

        return numbers;
    }

    private static bool IsLegacy(SqlConnection connection, string databaseName)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT CASE
                WHEN OBJECT_ID(@tasks, N'U') IS NOT NULL
                 AND OBJECT_ID(@groups, N'U') IS NOT NULL
                 AND OBJECT_ID(@users, N'U') IS NOT NULL
                 AND OBJECT_ID(@journal, N'U') IS NULL
                THEN 1 ELSE 0 END
            """;
        command.Parameters.AddWithValue("@tasks", databaseName + ".dbo.Tasks");
        command.Parameters.AddWithValue("@groups", databaseName + ".dbo.Groups");
        command.Parameters.AddWithValue("@users", databaseName + ".dbo.Users");
        command.Parameters.AddWithValue("@journal", databaseName + ".app.MigrationsJournal");
        return (int)command.ExecuteScalar()! == 1;
    }

    private static void CreateDatabase(SqlConnection connection, string databaseName)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandTimeout = 0;
        command.CommandText = "CREATE DATABASE " + Quote(databaseName);
        command.ExecuteNonQuery();
    }

    private static void ImportLegacy(string connectionString, string scriptPath, string sourceName, string targetName)
    {
        string script = File.ReadAllText(scriptPath);
        script = SetVarLine.Replace(script, string.Empty);
        script = script.Replace("$(SourceDb)", sourceName, StringComparison.Ordinal);
        script = script.Replace("$(TargetDb)", targetName, StringComparison.Ordinal);
        if (script.Contains("$(", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The legacy import script still contains an unsubstituted sqlcmd variable.");
        }

        DatabaseStartupLog.Write("Running legacy import script " + scriptPath);
        ExecuteBatches(connectionString, script);
    }

    private static void RunStep(string step, Action action)
    {
        DatabaseStartupLog.Write(step + " started.");
        try
        {
            action();
        }
        catch (Exception ex)
        {
            DatabaseStartupLog.Write(step + " failed: " + ex);
            throw;
        }

        DatabaseStartupLog.Write(step + " completed.");
    }

    private static void CloneDatabase(SqlConnection connection, string sourceName, string targetName)
    {
        string dataDirectory = PrimaryDataDirectory(connection, sourceName);
        string backupDirectory = BackupDirectory(connection) ?? dataDirectory;
        DatabaseStartupLog.Write("Backup folder: " + backupDirectory);
        string backupPath = Path.Combine(
            backupDirectory,
            targetName + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + ".bak");

        using (SqlCommand backup = connection.CreateCommand())
        {
            backup.CommandTimeout = 0;
            backup.CommandText = "BACKUP DATABASE " + Quote(sourceName)
                + " TO DISK = " + SqlString(backupPath)
                + " WITH COPY_ONLY, INIT";
            DatabaseStartupLog.Write("Backing up " + sourceName + " to " + backupPath);
            backup.ExecuteNonQuery();
        }

        List<BackupFile> files = ReadBackupFiles(connection, backupPath);
        string moves = string.Empty;
        int dataFiles = 0;
        for (int index = 0; index < files.Count; index++)
        {
            BackupFile file = files[index];
            bool isLog = string.Equals(file.Type, "L", StringComparison.OrdinalIgnoreCase);
            string extension;
            if (isLog)
            {
                extension = ".ldf";
            }
            else if (dataFiles == 0)
            {
                extension = ".mdf";
                dataFiles++;
            }
            else
            {
                extension = ".ndf";
                dataFiles++;
            }

            string safeName = string.Join("_", file.LogicalName.Split(Path.GetInvalidFileNameChars()));
            string physical = Path.Combine(dataDirectory, targetName + "_" + safeName + extension);
            if (moves.Length > 0)
            {
                moves += ", ";
            }

            moves += "MOVE " + Quote(file.LogicalName) + " TO " + SqlString(physical);
        }

        using (SqlCommand restore = connection.CreateCommand())
        {
            restore.CommandTimeout = 0;
            restore.CommandText = "RESTORE DATABASE " + Quote(targetName)
                + " FROM DISK = " + SqlString(backupPath)
                + " WITH " + moves + ", RECOVERY";
            DatabaseStartupLog.Write("Restoring " + targetName);
            restore.ExecuteNonQuery();
        }

        try
        {
            File.Delete(backupPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            DatabaseStartupLog.Write(
                "Warning: " + targetName + " was created, but the backup file could not be deleted: " + backupPath + ". " + ex.Message);
        }
    }

    private static List<BackupFile> ReadBackupFiles(SqlConnection connection, string backupPath)
    {
        List<BackupFile> files = new List<BackupFile>();
        using SqlCommand command = connection.CreateCommand();
        command.CommandTimeout = 0;
        command.CommandText = "RESTORE FILELISTONLY FROM DISK = " + SqlString(backupPath);
        using SqlDataReader reader = command.ExecuteReader();
        int logicalOrdinal = reader.GetOrdinal("LogicalName");
        int typeOrdinal = reader.GetOrdinal("Type");
        while (reader.Read())
        {
            files.Add(new BackupFile
            {
                LogicalName = reader.GetString(logicalOrdinal),
                Type = reader.GetString(typeOrdinal).Trim()
            });
        }

        if (files.Count == 0)
        {
            throw new InvalidOperationException("The database backup did not contain any files.");
        }

        return files;
    }

    private static string PrimaryDataDirectory(SqlConnection connection, string databaseName)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (1) mf.physical_name
            FROM sys.master_files AS mf
            WHERE mf.database_id = DB_ID(@name) AND mf.type = 0
            ORDER BY mf.file_id
            """;
        command.Parameters.AddWithValue("@name", databaseName);
        string? path = command.ExecuteScalar() as string;
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException($"Could not find data files for {Quote(databaseName)}.");
        }

        return Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException($"Could not find the data directory for {Quote(databaseName)}.");
    }

    private static string? BackupDirectory(SqlConnection connection)
    {
        try
        {
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = """
                DECLARE @path nvarchar(4000);
                EXEC master.dbo.xp_instance_regread
                    N'HKEY_LOCAL_MACHINE',
                    N'Software\Microsoft\MSSQLServer\MSSQLServer',
                    N'BackupDirectory',
                    @path OUTPUT;
                SELECT @path;
                """;
            return command.ExecuteScalar() as string;
        }
        catch (SqlException)
        {
            return null;
        }
    }

    private static void ExecuteBatches(string connectionString, string script)
    {
        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        int batchNumber = 0;
        foreach (string batch in SqlScriptSeeder.SplitIntoBatches(script))
        {
            if (string.IsNullOrWhiteSpace(batch))
            {
                continue;
            }

            batchNumber++;
            using SqlCommand command = connection.CreateCommand();
            command.CommandTimeout = 0;
            command.CommandText = """
                SET QUOTED_IDENTIFIER ON;
                SET ANSI_NULLS ON;
                """ + batch;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                string preview = batch.Trim().Replace("\r", " ").Replace("\n", " ");
                if (preview.Length > 180)
                {
                    preview = preview.Substring(0, 180);
                }

                DatabaseStartupLog.Write("Legacy import batch " + batchNumber + " failed: " + ex.Message + " | " + preview);
                throw;
            }
        }

        DatabaseStartupLog.Write("Legacy import finished. Batches executed: " + batchNumber);
    }

    private static string WithCatalog(string connectionString, string databaseName)
    {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = databaseName
        };
        return builder.ConnectionString;
    }

    private static string Quote(string name) => "[" + name.Replace("]", "]]", StringComparison.Ordinal) + "]";

    private static string SqlString(string value) => "N'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";

    private sealed class BackupFile
    {
        public string LogicalName { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
    }
}
