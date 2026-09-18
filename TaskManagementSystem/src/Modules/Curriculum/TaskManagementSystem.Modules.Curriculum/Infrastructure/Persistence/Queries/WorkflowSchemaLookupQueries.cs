using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

public sealed class WorkflowSchemaLookupQueries(CurriculumDbContext context) : IWorkflowSchemaLookup
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

        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var exists = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { SchemaId = schemaId }, cancellationToken: cancellationToken));
        return exists == 1;
    }
}
