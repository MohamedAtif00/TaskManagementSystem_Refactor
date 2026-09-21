using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Ticket.Features;
using DomainTaskStatus = TaskManagementSystem.Modules.Ticket.Domain.TaskStatus;
using DomainTaskPriority = TaskManagementSystem.Modules.Ticket.Domain.TaskPriority;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class SubjectTicketsQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<TicketListPageResult> ListBySubjectAsync(
        int subjectId,
        IReadOnlyList<DomainTaskStatus>? statuses = null,
        int? learningObjectiveId = null,
        string? name = null,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("SubjectId", subjectId);
        TicketListQueryBuilder.AddPaging(parameters, page, pageSize, out var paged, out var resolvedPage, out var resolvedPageSize);

        var where = new StringBuilder("""
            WHERE u.[SubjectId] = @SubjectId
              AND t.[Archived] = 0
              AND lo.[Archived] = 0
            """);
        TicketListQueryBuilder.AppendFilters(where, parameters, statuses, learningObjectiveId, name);

        var sql = $"""
            SELECT
                {TicketListQueryBuilder.SelectList}
            FROM [ticket].[Tasks] t
            INNER JOIN [curriculum].[LearningObjectives] lo ON t.[LearningObjectiveId] = lo.[Id]
            INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
            INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
            {where}
            {TicketListQueryBuilder.OrderAndPaging(paged)}
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = (await connection.QueryAsync<TicketRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).AsList();

        if (rows.Count == 0)
        {
            var emptyCount = 0;
            if (paged)
            {
                var countSql = $"""
                    SELECT COUNT(*)
                    FROM [ticket].[Tasks] t
                    INNER JOIN [curriculum].[LearningObjectives] lo ON t.[LearningObjectiveId] = lo.[Id]
                    INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
                    INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
                    {where}
                    """;
                emptyCount = await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
            }

            return new TicketListPageResult(
                [],
                paged ? resolvedPage : 1,
                paged ? resolvedPageSize : 0,
                emptyCount);
        }

        var items = rows.Select(TicketListItemResult.FromRow).ToList();
        return new TicketListPageResult(
            items,
            paged ? resolvedPage : 1,
            paged ? resolvedPageSize : items.Count,
            rows[0].TotalCount);
    }

    internal sealed record TicketRow(
        int Id,
        string Name,
        DomainTaskStatus Status,
        DomainTaskPriority Priority,
        int Duration,
        DateTime CreatedAt,
        int LearningObjectiveId,
        int? StepId,
        int? UserId,
        int? TeamId,
        bool Pause,
        bool Attention,
        bool Flagged,
        bool IsRollback,
        int RollbackCount,
        int TotalCount);
}
