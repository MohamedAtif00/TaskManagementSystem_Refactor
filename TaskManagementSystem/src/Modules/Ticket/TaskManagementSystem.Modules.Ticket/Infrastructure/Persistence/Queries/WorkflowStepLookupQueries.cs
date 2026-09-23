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

    public async Task<IReadOnlyList<WorkflowStepSummary>> ListStartStepsAsync(
        int schemaId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH Ranked AS (
                SELECT
                    s.[Id],
                    s.[Duration],
                    s.[Priority],
                    s.[NodeId],
                    s.[TicketBankId],
                    ROW_NUMBER() OVER (PARTITION BY n.[Id] ORDER BY s.[Order], s.[Id]) AS RowNum
                FROM [workflows].[Steps] s
                INNER JOIN [workflows].[Nodes] n ON s.[NodeId] = n.[Id]
                WHERE n.[SchemaId] = @SchemaId
                  AND n.[Archived] = 0
                  AND n.[isStart] = 1
                  AND s.[Archived] = 0
            )
            SELECT [Id], [Duration], [Priority], [NodeId], [TicketBankId]
            FROM Ranked
            WHERE RowNum = 1
            ORDER BY [NodeId]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = await connection.QueryAsync<WorkflowStepSummary>(
            new CommandDefinition(sql, new { SchemaId = schemaId }, cancellationToken: cancellationToken));
        return rows.ToList();
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

    public async Task<WorkflowStepSummary?> GetNextStepInNodeAsync(
        int currentStepId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1
                nextStep.[Id],
                nextStep.[Duration],
                nextStep.[Priority],
                nextStep.[NodeId],
                nextStep.[TicketBankId]
            FROM [workflows].[Steps] currentStep
            INNER JOIN [workflows].[Steps] nextStep
                ON nextStep.[NodeId] = currentStep.[NodeId]
               AND nextStep.[Archived] = 0
               AND nextStep.[Order] = currentStep.[Order] + 1
            WHERE currentStep.[Id] = @CurrentStepId
              AND currentStep.[Archived] = 0
            ORDER BY nextStep.[Id]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<WorkflowStepSummary>(
            new CommandDefinition(sql, new { CurrentStepId = currentStepId }, cancellationToken: cancellationToken));
    }

    public async Task<WorkflowStepSummary?> GetFirstStepInNodeAsync(
        int nodeId,
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
            WHERE s.[NodeId] = @NodeId
              AND s.[Archived] = 0
              AND s.[Order] = 1
            ORDER BY s.[Id]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<WorkflowStepSummary>(
            new CommandDefinition(sql, new { NodeId = nodeId }, cancellationToken: cancellationToken));
    }

    public async Task<WorkflowStepSummary?> GetLastStepInNodeAsync(
        int nodeId,
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
            WHERE s.[NodeId] = @NodeId
              AND s.[Archived] = 0
            ORDER BY s.[Order] DESC, s.[Id] DESC
            """;

        using var connection = connectionFactory.GetOpenConnection();
        return await connection.QuerySingleOrDefaultAsync<WorkflowStepSummary>(
            new CommandDefinition(sql, new { NodeId = nodeId }, cancellationToken: cancellationToken));
    }

    public Task<IReadOnlyList<int>> ListNextNodeIdsAsync(
        int nodeId,
        CancellationToken cancellationToken = default) =>
        ListLinkedNodeIdsAsync(
            """
            SELECT link.[NextId]
            FROM [workflows].[NodeSequences] link
            INNER JOIN [workflows].[Nodes] node ON node.[Id] = link.[NextId]
            WHERE link.[PreviousId] = @NodeId
              AND node.[Archived] = 0
            """,
            nodeId,
            cancellationToken);

    public Task<IReadOnlyList<int>> ListPreviousNodeIdsAsync(
        int nodeId,
        CancellationToken cancellationToken = default) =>
        ListLinkedNodeIdsAsync(
            """
            SELECT link.[PreviousId]
            FROM [workflows].[NodeSequences] link
            INNER JOIN [workflows].[Nodes] node ON node.[Id] = link.[PreviousId]
            WHERE link.[NextId] = @NodeId
              AND node.[Archived] = 0
            """,
            nodeId,
            cancellationToken);

    private async Task<IReadOnlyList<int>> ListLinkedNodeIdsAsync(
        string sql,
        int nodeId,
        CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.GetOpenConnection();
        var rows = await connection.QueryAsync<int>(
            new CommandDefinition(sql, new { NodeId = nodeId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }
}
