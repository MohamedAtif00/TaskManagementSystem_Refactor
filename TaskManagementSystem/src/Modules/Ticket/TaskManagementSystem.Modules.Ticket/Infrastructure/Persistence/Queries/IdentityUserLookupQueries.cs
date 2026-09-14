using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class IdentityUserLookupQueries(ISqlConnectionFactory connectionFactory)
    : IIdentityUserLookup
{
    public async Task<bool> ActiveUserExistsAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM [identity].[Users]
                WHERE [Id] = @UserId AND [Archived] = 0
            ) THEN 1 ELSE 0 END
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }
}
