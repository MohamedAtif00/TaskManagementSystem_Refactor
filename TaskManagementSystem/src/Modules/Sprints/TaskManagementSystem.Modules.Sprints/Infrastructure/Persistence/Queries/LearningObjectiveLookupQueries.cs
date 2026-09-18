using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Infrastructure.Persistence.Queries;

public sealed class LearningObjectiveLookupQueries(ISqlConnectionFactory connectionFactory)
    : ILearningObjectiveLookup
{
    public async Task<bool> ActiveLearningObjectivesExistAsync(
        IReadOnlyCollection<int> learningObjectiveIds,
        CancellationToken cancellationToken = default)
    {
        if (learningObjectiveIds.Count == 0)
        {
            return true;
        }

        const string sql = """
            SELECT COUNT(*)
            FROM [curriculum].[LearningObjectives]
            WHERE [Archived] = 0 AND [Id] IN @LearningObjectiveIds
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                sql,
                new { LearningObjectiveIds = learningObjectiveIds.Distinct().ToArray() },
                cancellationToken: cancellationToken));

        return count == learningObjectiveIds.Distinct().Count();
    }
}
