using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

public sealed class IdentityUserLookupQueries(ISqlConnectionFactory connectionFactory)
    : IIdentityUserLookup
{
    public async Task<bool> ActiveUsersExistAsync(
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return true;
        }

        const string sql = """
            SELECT COUNT(*)
            FROM [identity].[Users]
            WHERE [Archived] = 0 AND [Id] IN @UserIds
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { UserIds = userIds.Distinct().ToArray() }, cancellationToken: cancellationToken));

        return count == userIds.Distinct().Count();
    }

    public async Task<IReadOnlyList<UserSummary>> GetActiveUsersByIdsAsync(
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        const string sql = """
            SELECT [Id], [Name]
            FROM [identity].[Users]
            WHERE [Archived] = 0 AND [Id] IN @UserIds
            ORDER BY [Name]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var users = await connection.QueryAsync<UserSummary>(
            new CommandDefinition(sql, new { UserIds = userIds.Distinct().ToArray() }, cancellationToken: cancellationToken));

        return users.ToList();
    }
}
