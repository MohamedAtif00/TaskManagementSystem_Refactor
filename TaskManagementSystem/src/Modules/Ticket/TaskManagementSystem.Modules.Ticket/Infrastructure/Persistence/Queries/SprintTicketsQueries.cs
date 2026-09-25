using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Ticket.Features;
using DomainTicketStatus = TaskManagementSystem.Modules.Ticket.Domain.TicketStatus;
using DomainTicketPriority = TaskManagementSystem.Modules.Ticket.Domain.TicketPriority;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class SprintTicketsQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<TicketListPageResult> ListBySprintAsync(
        int sprintId,
        IReadOnlyList<DomainTicketStatus>? statuses = null,
        int? learningObjectiveId = null,
        string? name = null,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("SprintId", sprintId);
        TicketListQueryBuilder.AddPaging(parameters, page, pageSize, out _, out var resolvedPage, out var resolvedPageSize);

        var where = new StringBuilder("""
            WHERE slo.[SprintId] = @SprintId
              AND t.[Archived] = 0
            """);
        TicketListQueryBuilder.AppendFilters(where, parameters, statuses, learningObjectiveId, name);

        var fromSql = """
            FROM [ticket].[Tickets] t
            INNER JOIN [sprints].[SprintLearningObjectives] slo ON t.[LearningObjectiveId] = slo.[LearningObjectiveId]
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
        return new TicketListPageResult(
            items,
            resolvedPage,
            resolvedPageSize,
            totalCount);
    }

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
        int RollbackCount);
}
