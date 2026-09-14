using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

public sealed class WorkflowSchemaLookupQueries(ISqlConnectionFactory connectionFactory)
    : IWorkflowSchemaLookup
{
    public async Task<bool> ActiveSchemaExistsAsync(int schemaId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM [workflows].[Schemas]
                WHERE [Id] = @SchemaId AND [Archived] = 0
            ) THEN 1 ELSE 0 END
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { SchemaId = schemaId }, cancellationToken: cancellationToken));
    }
}
