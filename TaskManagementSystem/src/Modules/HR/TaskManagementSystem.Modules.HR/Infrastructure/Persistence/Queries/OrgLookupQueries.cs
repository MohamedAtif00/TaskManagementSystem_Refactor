using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

internal sealed class OrgLookupQueries(ISqlConnectionFactory connectionFactory)
{
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
