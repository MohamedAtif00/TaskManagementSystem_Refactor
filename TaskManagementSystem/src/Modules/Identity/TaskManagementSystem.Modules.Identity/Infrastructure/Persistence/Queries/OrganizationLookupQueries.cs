using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;

namespace TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Queries;

public sealed class OrganizationLookupQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<bool> ActiveTeamExistsAsync(int teamId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM [organization].[Teams]
                WHERE [Id] = @TeamId AND [Archived] = 0
            ) THEN 1 ELSE 0 END
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { TeamId = teamId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> IsActiveSectionHeadAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM [organization].[Sections]
                WHERE [HeadId] = @UserId AND [Archived] = 0
            ) THEN 1 ELSE 0 END
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }
}
