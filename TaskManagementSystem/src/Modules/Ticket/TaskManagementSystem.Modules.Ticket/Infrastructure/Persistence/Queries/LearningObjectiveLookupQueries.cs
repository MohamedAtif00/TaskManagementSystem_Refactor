using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class LearningObjectiveLookupQueries(ISqlConnectionFactory connectionFactory)
    : ILearningObjectiveLookup
{
    public async Task<LearningObjectiveSummary?> GetActiveByIdAsync(
        int learningObjectiveId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT [Id], [SchemaId]
            FROM [curriculum].[LearningObjectives]
            WHERE [Id] = @LearningObjectiveId AND [Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<LearningObjectiveSummary>(
            new CommandDefinition(sql, new { LearningObjectiveId = learningObjectiveId }, cancellationToken: cancellationToken));
    }
}
