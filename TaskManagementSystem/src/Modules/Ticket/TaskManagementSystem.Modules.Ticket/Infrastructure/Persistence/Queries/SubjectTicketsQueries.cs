using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using DomainTicketStatus = TaskManagementSystem.Modules.Ticket.Domain.TicketStatus;
using DomainTicketPriority = TaskManagementSystem.Modules.Ticket.Domain.TicketPriority;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class SubjectTicketsQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<Result<TicketListPageResult>> ListBySubjectAsync(
        int subjectId,
        TicketListFilter filter,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("SubjectId", subjectId);
        var paging = TicketListQueryBuilder.AddPaging(parameters, page, pageSize);
        if (paging.IsFailure)
        {
            return Result.Fail<TicketListPageResult>(paging.Error);
        }

        var (resolvedPage, resolvedPageSize) = paging.Value;

        var where = new StringBuilder("""
            WHERE u.[SubjectId] = @SubjectId
              AND t.[Archived] = 0
              AND lo.[Archived] = 0
            """);
        TicketListQueryBuilder.AppendFilters(where, parameters, filter);

        var fromSql = """
            FROM [ticket].[Tickets] t
            INNER JOIN [curriculum].[LearningObjectives] lo ON t.[LearningObjectiveId] = lo.[Id]
            INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
            INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
            """;

        var sql = $"""
            SELECT
                {TicketListQueryBuilder.SelectList}
            {fromSql}
            {where}
            {TicketListQueryBuilder.OrderAndPaging(paged: true)}
            """;

        var countSql = $"""
            SELECT COUNT(*)
            {fromSql}
            {where}
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));

        var rows = (await connection.QueryAsync<TicketRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).AsList();

        var items = rows.Select(TicketListItemResult.FromRow).ToList();
        return Result.Ok(new TicketListPageResult(
            items,
            resolvedPage,
            resolvedPageSize,
            totalCount));
    }

    public async Task<IReadOnlyList<TicketAssignmentLinkResult>> ListAssignmentLinksAsync(
        int subjectId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("SubjectId", subjectId);
        var sql = """
            SELECT DISTINCT t.[UserId], t.[LearningObjectiveId]
            FROM [ticket].[Tickets] t
            INNER JOIN [curriculum].[LearningObjectives] lo ON t.[LearningObjectiveId] = lo.[Id]
            INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
            INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
            WHERE u.[SubjectId] = @SubjectId
              AND t.[Archived] = 0
              AND lo.[Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = (await connection.QueryAsync<AssignmentLinkRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).AsList();
        return rows
            .Select(row => new TicketAssignmentLinkResult(row.UserId, row.LearningObjectiveId))
            .ToArray();
    }

    internal sealed record AssignmentLinkRow(int? UserId, int LearningObjectiveId);

    internal sealed record TicketRow(
        int Id,
        string Name,
        DomainTicketStatus Status,
        DomainTicketPriority Priority,
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
        string LearningObjectiveName);
}
