using Microsoft.Data.SqlClient;

namespace TaskManagementSystem.Modules.HR.UnitTests;

internal static class LocalDbFact
{
    private static readonly Lazy<bool> IsAvailable = new(CheckAvailability);

    public static bool IsLocalDbAvailable => IsAvailable.Value;

    private static bool CheckAvailability()
    {
        try
        {
            using var connection = new SqlConnection(
                "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=2");
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
