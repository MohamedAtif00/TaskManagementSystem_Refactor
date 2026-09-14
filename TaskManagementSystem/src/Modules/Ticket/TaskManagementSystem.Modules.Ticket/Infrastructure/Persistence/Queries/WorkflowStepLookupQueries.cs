using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class WorkflowStepLookupQueries(ISqlConnectionFactory connectionFactory)
    : IWorkflowStepLookup
{
    public async Task<WorkflowStepSummary?> GetFirstStepAsync(
        int schemaId,
        int taskBankId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1
                s.[Id],
                s.[Duration],
                s.[Priority],
                s.[NodeId],
                s.[TaskBankId]
            FROM [workflows].[Steps] s
            INNER JOIN [workflows].[Nodes] n ON s.[NodeId] = n.[Id]
            WHERE n.[SchemaId] = @SchemaId
              AND n.[Archived] = 0
              AND s.[Archived] = 0
              AND s.[TaskBankId] = @TaskBankId
              AND (
                  n.[IsStart] = 1
                  OR NOT EXISTS (
                      SELECT 1
                      FROM [workflows].[Nodes] startNode
                      WHERE startNode.[SchemaId] = @SchemaId
                        AND startNode.[IsStart] = 1
                        AND startNode.[Archived] = 0))
            ORDER BY
                CASE WHEN n.[IsStart] = 1 THEN 0 ELSE 1 END,
                n.[Order],
                s.[Order]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<WorkflowStepSummary>(
            new CommandDefinition(
                sql,
                new { SchemaId = schemaId, TaskBankId = taskBankId },
                cancellationToken: cancellationToken));
    }

    public async Task<int?> GetNextStepIdAsync(
        int schemaId,
        int currentStepId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH OrderedSteps AS (
                SELECT
                    s.[Id],
                    ROW_NUMBER() OVER (ORDER BY n.[Order], s.[Order]) AS RowNum
                FROM [workflows].[Steps] s
                INNER JOIN [workflows].[Nodes] n ON s.[NodeId] = n.[Id]
                WHERE n.[SchemaId] = @SchemaId
                  AND n.[Archived] = 0
                  AND s.[Archived] = 0
            )
            SELECT nextStep.[Id]
            FROM OrderedSteps currentStep
            INNER JOIN OrderedSteps nextStep ON nextStep.RowNum = currentStep.RowNum + 1
            WHERE currentStep.[Id] = @CurrentStepId
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(
                sql,
                new { SchemaId = schemaId, CurrentStepId = currentStepId },
                cancellationToken: cancellationToken));
    }

    public async Task<WorkflowStepSummary?> GetActiveByIdAsync(
        int stepId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                s.[Id],
                s.[Duration],
                s.[Priority],
                s.[NodeId],
                s.[TaskBankId]
            FROM [workflows].[Steps] s
            WHERE s.[Id] = @StepId AND s.[Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<WorkflowStepSummary>(
            new CommandDefinition(sql, new { StepId = stepId }, cancellationToken: cancellationToken));
    }
}
