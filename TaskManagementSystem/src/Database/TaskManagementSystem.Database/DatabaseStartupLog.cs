using System;
using System.IO;
using DbUp.Engine.Output;

namespace TaskManagementSystem.Database;

public sealed class DatabaseStartupLog : IUpgradeLog
{
    public static void Write(string message)
    {
        string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message;
        Console.WriteLine(line);
        try
        {
            string directory = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(directory);
            File.AppendAllText(Path.Combine(directory, "database-version.log"), line + Environment.NewLine);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.WriteLine("Could not write logs\\database-version.log: " + ex.Message);
        }
    }

    public void LogTrace(string format, params object[] args)
    {
    }

    public void LogDebug(string format, params object[] args)
    {
    }

    public void LogInformation(string format, params object[] args) => Write(Format(format, args));

    public void LogWarning(string format, params object[] args) => Write("WARN " + Format(format, args));

    public void LogError(string format, params object[] args) => Write("ERROR " + Format(format, args));

    public void LogError(Exception ex, string format, params object[] args) =>
        Write("ERROR " + Format(format, args) + Environment.NewLine + ex);

    private static string Format(string format, object[] args)
    {
        if (args is null || args.Length == 0)
        {
            return format;
        }

        try
        {
            return string.Format(format, args);
        }
        catch (FormatException)
        {
            return format;
        }
    }
}
