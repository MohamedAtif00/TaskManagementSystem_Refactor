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

    public async Task<ActiveUserRecord?> GetActiveUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT [Id], [TeamId]
            FROM [identity].[Users]
            WHERE [Id] = @UserId AND [Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<ActiveUserRecord>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyDictionary<int, string>> ListNamesAsync(
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<int, string>();
        }

        const string sql = """
            SELECT [Id], [Name]
            FROM [identity].[Users]
            WHERE [Id] IN @UserIds
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = await connection.QueryAsync<UserSummary>(
            new CommandDefinition(sql, new { UserIds = userIds }, cancellationToken: cancellationToken));
        return rows
            .GroupBy(row => row.Id)
            .ToDictionary(group => group.Key, group => group.First().Name);
    }
}
