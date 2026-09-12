using System.Data;
using Microsoft.Data.SqlClient;
using TaskManagementSystem.BuildingBlocks.Application.Data;

namespace TaskManagementSystem.BuildingBlocks.Persistence.Data;

public sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public string GetConnectionString() => connectionString;

    public IDbConnection GetOpenConnection()
    {
        var connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}
