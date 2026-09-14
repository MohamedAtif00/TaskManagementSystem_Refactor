using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class OrganizationTeamLookupQueries(ISqlConnectionFactory connectionFactory)
    : IOrganizationTeamLookup
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
}
