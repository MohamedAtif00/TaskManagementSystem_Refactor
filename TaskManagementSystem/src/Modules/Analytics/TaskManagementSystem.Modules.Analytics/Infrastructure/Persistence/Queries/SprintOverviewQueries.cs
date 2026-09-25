using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Analytics.Application;
using TaskManagementSystem.Modules.Analytics.Features;

namespace TaskManagementSystem.Modules.Analytics.Infrastructure.Persistence.Queries;

public sealed class SprintOverviewQueries(ISqlConnectionFactory connectionFactory)
{
    private const string AggregateSql = """
        ;WITH [ScopeLos] AS (
            SELECT DISTINCT lo.[Id] AS [LearningObjectiveId]
            FROM [sprints].[SprintLearningObjectives] slo
            INNER JOIN [curriculum].[LearningObjectives] lo ON slo.[LearningObjectiveId] = lo.[Id]
            INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
            INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
            INNER JOIN [curriculum].[Subjects] s ON u.[SubjectId] = s.[Id]
            WHERE slo.[SprintId] = @SprintId
              AND lo.[Archived] = 0
              AND l.[Archived] = 0
              AND u.[Archived] = 0
              AND s.[Archived] = 0
        ),
        [LatestTickets] AS (
            SELECT
                t.[LearningObjectiveId],
                t.[Status],
                ROW_NUMBER() OVER (
                    PARTITION BY t.[LearningObjectiveId]
                    ORDER BY t.[CreatedAt] DESC, t.[Id] DESC
                ) AS [RowNum]
            FROM [ticket].[Tickets] t
            WHERE t.[Archived] = 0
              AND EXISTS (
                  SELECT 1
                  FROM [ScopeLos] sl
                  WHERE sl.[LearningObjectiveId] = t.[LearningObjectiveId]
              )
        ),
        [LoClassified] AS (
            SELECT
                sl.[LearningObjectiveId],
                lt.[Status]
            FROM [ScopeLos] sl
            LEFT JOIN [LatestTickets] lt
                ON sl.[LearningObjectiveId] = lt.[LearningObjectiveId]
               AND lt.[RowNum] = 1
        ),
        [LoAgg] AS (
            SELECT
                COUNT(*) AS [TotalLearningObjectives],
                SUM(CASE
                    WHEN [Status] IS NULL OR [Status] <= 1 OR [Status] NOT IN (2, 3) THEN 1
                    ELSE 0
                END) AS [IdleLearningObjectives],
                SUM(CASE WHEN [Status] = 2 THEN 1 ELSE 0 END) AS [RunningLearningObjectives],
                SUM(CASE WHEN [Status] = 3 THEN 1 ELSE 0 END) AS [DoneLearningObjectives]
            FROM [LoClassified]
        ),
        [TicketAgg] AS (
            SELECT
                COUNT(DISTINCT CASE WHEN t.[Status] = 0 THEN t.[Id] END) AS [BacklogTickets],
                COUNT(DISTINCT CASE WHEN t.[Status] = 1 THEN t.[Id] END) AS [ToDoTickets],
                COUNT(DISTINCT CASE WHEN t.[Status] = 2 THEN t.[Id] END) AS [DoingTickets],
                COUNT(DISTINCT CASE WHEN t.[Status] = 3 THEN t.[Id] END) AS [DoneTickets]
            FROM [ticket].[Tickets] t
            WHERE t.[Archived] = 0
              AND EXISTS (
                  SELECT 1
                  FROM [sprints].[SprintLearningObjectives] slo
                  INNER JOIN [curriculum].[LearningObjectives] lo ON slo.[LearningObjectiveId] = lo.[Id]
                  INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
                  INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
                  INNER JOIN [curriculum].[Subjects] s ON u.[SubjectId] = s.[Id]
                  WHERE slo.[SprintId] = @SprintId
                    AND slo.[LearningObjectiveId] = t.[LearningObjectiveId]
                    AND lo.[Archived] = 0
                    AND l.[Archived] = 0
                    AND u.[Archived] = 0
                    AND s.[Archived] = 0
              )
        )
        SELECT
            la.[TotalLearningObjectives],
            la.[IdleLearningObjectives],
            la.[RunningLearningObjectives],
            la.[DoneLearningObjectives],
            ta.[BacklogTickets],
            ta.[ToDoTickets],
            ta.[DoingTickets],
            ta.[DoneTickets]
        FROM [LoAgg] la
        CROSS JOIN [TicketAgg] ta
        """;

    public async Task<Result<OverviewResult>> GetBySprintIdAsync(
        int sprintId,
        CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.GetOpenConnection();

        var exists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM [sprints].[Sprints]
                    WHERE [Id] = @SprintId
                      AND [IsArchived] = 0
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
                """,
                new { SprintId = sprintId },
                cancellationToken: cancellationToken));

        if (!exists)
        {
            return Result.Fail<OverviewResult>(AnalyticsErrors.SprintNotFound);
        }

        var row = await connection.QuerySingleAsync<OverviewAggregateRow>(
            new CommandDefinition(
                AggregateSql,
                new { SprintId = sprintId },
                cancellationToken: cancellationToken));

        return Result.Ok(MapResult(sprintId, "sprint", row));
    }

    private static OverviewResult MapResult(int scopeId, string scopeType, OverviewAggregateRow row)
    {
        var progressPercent = row.TotalLearningObjectives > 0
            ? (int)Math.Round(row.DoneLearningObjectives * 100.0 / row.TotalLearningObjectives)
            : 0;

        return new OverviewResult(
            scopeId,
            scopeType,
            row.TotalLearningObjectives,
            row.IdleLearningObjectives,
            row.RunningLearningObjectives,
            row.DoneLearningObjectives,
            progressPercent,
            row.BacklogTickets,
            row.ToDoTickets,
            row.DoingTickets,
            row.DoneTickets,
            DateTime.UtcNow);
    }

    private sealed class OverviewAggregateRow
    {
        public int TotalLearningObjectives { get; init; }
        public int IdleLearningObjectives { get; init; }
        public int RunningLearningObjectives { get; init; }
        public int DoneLearningObjectives { get; init; }
        public int BacklogTickets { get; init; }
        public int ToDoTickets { get; init; }
        public int DoingTickets { get; init; }
        public int DoneTickets { get; init; }
    }
}
