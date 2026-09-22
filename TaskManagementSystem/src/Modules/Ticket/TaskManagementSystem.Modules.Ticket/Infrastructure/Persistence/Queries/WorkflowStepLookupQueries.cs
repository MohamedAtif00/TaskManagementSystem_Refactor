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
                s.[TicketBankId]
            FROM [workflows].[Steps] s
            INNER JOIN [workflows].[Nodes] n ON s.[NodeId] = n.[Id]
            WHERE n.[SchemaId] = @SchemaId
              AND n.[Archived] = 0
              AND s.[Archived] = 0
              AND s.[TicketBankId] = @TicketBankId
              AND (
                  n.[isStart] = 1
                  OR NOT EXISTS (
                      SELECT 1
                      FROM [workflows].[Nodes] startNode
                      WHERE startNode.[SchemaId] = @SchemaId
                        AND startNode.[isStart] = 1
                        AND startNode.[Archived] = 0))
            ORDER BY
                CASE WHEN n.[isStart] = 1 THEN 0 ELSE 1 END,
                n.[Order],
                s.[Order]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<WorkflowStepSummary>(
            new CommandDefinition(
                sql,
                new { SchemaId = schemaId, TicketBankId = taskBankId },
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
                s.[TicketBankId]
            FROM [workflows].[Steps] s
            WHERE s.[Id] = @StepId AND s.[Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<WorkflowStepSummary>(
            new CommandDefinition(sql, new { StepId = stepId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<WorkflowJumpPoint>> ListStepsAheadAsync(
        int schemaId,
        int currentStepId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH OrderedSteps AS (
                SELECT
                    s.[Id] AS StepId,
                    s.[NodeId] AS NodeId,
                    n.[Name] AS NodeName,
                    ROW_NUMBER() OVER (ORDER BY n.[Order], s.[Order]) AS RowNum
                FROM [workflows].[Steps] s
                INNER JOIN [workflows].[Nodes] n ON s.[NodeId] = n.[Id]
                WHERE n.[SchemaId] = @SchemaId
                  AND n.[Archived] = 0
                  AND s.[Archived] = 0
            )
            SELECT ahead.StepId, ahead.NodeId, ahead.NodeName AS Label
            FROM OrderedSteps currentStep
            INNER JOIN OrderedSteps ahead ON ahead.RowNum > currentStep.RowNum
            WHERE currentStep.StepId = @CurrentStepId
            ORDER BY ahead.RowNum
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = await connection.QueryAsync<(int StepId, int NodeId, string Label)>(
            new CommandDefinition(
                sql,
                new { SchemaId = schemaId, CurrentStepId = currentStepId },
                cancellationToken: cancellationToken));
        return rows.Select(row => new WorkflowJumpPoint(row.StepId, row.NodeId, row.Label)).ToList();
    }

    public async Task<bool> IsStepAheadAsync(
        int schemaId,
        int currentStepId,
        int targetStepId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH OrderedSteps AS (
                SELECT
                    s.[Id] AS StepId,
                    ROW_NUMBER() OVER (ORDER BY n.[Order], s.[Order]) AS RowNum
                FROM [workflows].[Steps] s
                INNER JOIN [workflows].[Nodes] n ON s.[NodeId] = n.[Id]
                WHERE n.[SchemaId] = @SchemaId
                  AND n.[Archived] = 0
                  AND s.[Archived] = 0
            )
            SELECT CASE WHEN targetStep.RowNum > currentStep.RowNum THEN 1 ELSE 0 END
            FROM OrderedSteps currentStep
            INNER JOIN OrderedSteps targetStep ON targetStep.StepId = @TargetStepId
            WHERE currentStep.StepId = @CurrentStepId
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                new { SchemaId = schemaId, CurrentStepId = currentStepId, TargetStepId = targetStepId },
                cancellationToken: cancellationToken));
    }
}
