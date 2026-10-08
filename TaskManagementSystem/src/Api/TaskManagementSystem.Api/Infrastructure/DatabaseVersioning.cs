using TaskManagementSystem.Database;

namespace TaskManagementSystem.Api.Infrastructure;

public static class DatabaseVersioning
{
    public static bool Enabled { get; set; } = true;

    public static void Log(string message) => DatabaseStartupLog.Write(message);
}
