using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

public sealed class OrgLookupQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<int?> GetTeamleaderIdForTeamAsync(int? teamId, CancellationToken cancellationToken = default)
    {
        if (teamId is null)
        {
            return null;
        }

        const string sql = """
            SELECT [TeamleaderId]
            FROM [organization].[Teams]
            WHERE [Id] = @TeamId AND [Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<int?>(
            new CommandDefinition(sql, new { TeamId = teamId.Value }, cancellationToken: cancellationToken));
    }

    public async Task<int?> GetSectionHeadIdForTeamAsync(int teamId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT s.[HeadId]
            FROM [organization].[SectionTeams] AS st
            INNER JOIN [organization].[Sections] AS s ON s.[Id] = st.[SectionId]
            WHERE st.[TeamId] = @TeamId
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<int?>(
            new CommandDefinition(sql, new { TeamId = teamId }, cancellationToken: cancellationToken));
    }

}
