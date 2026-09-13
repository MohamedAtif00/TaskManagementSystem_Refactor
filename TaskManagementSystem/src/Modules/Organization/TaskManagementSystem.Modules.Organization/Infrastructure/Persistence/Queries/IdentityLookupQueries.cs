using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Organization.Application;

namespace TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

public sealed class IdentityLookupQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<UserSummary?> GetActiveUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT [Id], [Name]
            FROM [identity].[Users]
            WHERE [Id] = @UserId AND [Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<UserSummary>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<UserSummary>> GetActiveTeamMembersAsync(
        int teamId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT [Id], [Name]
            FROM [identity].[Users]
            WHERE [TeamId] = @TeamId AND [Archived] = 0
            ORDER BY [Name]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var members = await connection.QueryAsync<UserSummary>(
            new CommandDefinition(sql, new { TeamId = teamId }, cancellationToken: cancellationToken));

        return members.ToList();
    }

    public async Task<IReadOnlyDictionary<int, int>> GetMemberCountsByTeamIdsAsync(
        IReadOnlyCollection<int> teamIds,
        CancellationToken cancellationToken = default)
    {
        if (teamIds.Count == 0)
        {
            return new Dictionary<int, int>();
        }

        const string sql = """
            SELECT [TeamId] AS [Key], COUNT(*) AS [Value]
            FROM [identity].[Users]
            WHERE [Archived] = 0 AND [TeamId] IN @TeamIds
            GROUP BY [TeamId]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var counts = await connection.QueryAsync<(int Key, int Value)>(
            new CommandDefinition(sql, new { TeamIds = teamIds.Distinct().ToArray() }, cancellationToken: cancellationToken));

        return counts.ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}
