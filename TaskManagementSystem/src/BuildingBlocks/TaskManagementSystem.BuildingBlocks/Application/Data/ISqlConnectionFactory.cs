using System.Data;

namespace TaskManagementSystem.BuildingBlocks.Application.Data;

public interface ISqlConnectionFactory
{
    IDbConnection GetOpenConnection();

    string GetConnectionString();
}
