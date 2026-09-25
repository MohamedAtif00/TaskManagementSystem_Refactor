using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketSummary;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class TicketSummaryQueries(ISqlConnectionFactory connectionFactory)
{
    private const string SubjectSummarySql = """
        SELECT
            COUNT(DISTINCT CASE WHEN t.[Status] = 0 THEN t.[Id] END) AS [Backlog],
            COUNT(DISTINCT CASE WHEN t.[Status] = 1 THEN t.[Id] END) AS [ToDo],
            COUNT(DISTINCT CASE WHEN t.[Status] = 2 THEN t.[Id] END) AS [Doing],
            COUNT(DISTINCT CASE WHEN t.[Status] = 3 THEN t.[Id] END) AS [Done],
            COUNT(DISTINCT t.[Id]) AS [TotalCount]
        FROM [ticket].[Tickets] t
        INNER JOIN [curriculum].[LearningObjectives] lo ON t.[LearningObjectiveId] = lo.[Id]
        INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
        INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
        INNER JOIN [curriculum].[Subjects] s ON u.[SubjectId] = s.[Id]
        WHERE u.[SubjectId] = @SubjectId
          AND t.[Archived] = 0
          AND lo.[Archived] = 0
          AND l.[Archived] = 0
          AND u.[Archived] = 0
          AND s.[Archived] = 0
        """;

    private const string SprintSummarySql = """
        SELECT
            COUNT(DISTINCT CASE WHEN t.[Status] = 0 THEN t.[Id] END) AS [Backlog],
            COUNT(DISTINCT CASE WHEN t.[Status] = 1 THEN t.[Id] END) AS [ToDo],
            COUNT(DISTINCT CASE WHEN t.[Status] = 2 THEN t.[Id] END) AS [Doing],
            COUNT(DISTINCT CASE WHEN t.[Status] = 3 THEN t.[Id] END) AS [Done],
            COUNT(DISTINCT t.[Id]) AS [TotalCount]
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
        """;

    public async Task<Result<TicketSummaryResult>> GetBySubjectIdAsync(
        int subjectId,
        CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.GetOpenConnection();

        var exists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM [curriculum].[Subjects]
                    WHERE [Id] = @SubjectId
                      AND [Archived] = 0
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
                """,
                new { SubjectId = subjectId },
                cancellationToken: cancellationToken));

        if (!exists)
        {
            return Result.Fail<TicketSummaryResult>(TicketErrors.SubjectNotFound);
        }

        var row = await connection.QuerySingleAsync<TicketSummaryRow>(
            new CommandDefinition(
                SubjectSummarySql,
                new { SubjectId = subjectId },
                cancellationToken: cancellationToken));

        return Result.Ok(MapResult(row));
    }

    public async Task<Result<TicketSummaryResult>> GetBySprintIdAsync(
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
            return Result.Fail<TicketSummaryResult>(TicketErrors.SprintNotFound);
        }

        var row = await connection.QuerySingleAsync<TicketSummaryRow>(
            new CommandDefinition(
                SprintSummarySql,
                new { SprintId = sprintId },
                cancellationToken: cancellationToken));

        return Result.Ok(MapResult(row));
    }

    private static TicketSummaryResult MapResult(TicketSummaryRow row) =>
        new(
            row.Backlog,
            row.ToDo,
            row.Doing,
            row.Done,
            row.TotalCount,
            DateTime.UtcNow);

    private sealed class TicketSummaryRow
    {
        public int Backlog { get; init; }
        public int ToDo { get; init; }
        public int Doing { get; init; }
        public int Done { get; init; }
        public int TotalCount { get; init; }
    }
}
