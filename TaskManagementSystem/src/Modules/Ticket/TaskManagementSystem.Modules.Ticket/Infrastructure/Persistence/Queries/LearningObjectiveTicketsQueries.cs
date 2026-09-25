using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using DomainTicketStatus = TaskManagementSystem.Modules.Ticket.Domain.TicketStatus;
using DomainTicketPriority = TaskManagementSystem.Modules.Ticket.Domain.TicketPriority;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class LearningObjectiveTicketsQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<Result<TicketListPageResult>> ListByLearningObjectiveAsync(
        int learningObjectiveId,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("LearningObjectiveId", learningObjectiveId);
        var paging = TicketListQueryBuilder.AddPaging(parameters, page, pageSize);
        if (paging.IsFailure)
        {
            return Result.Fail<TicketListPageResult>(paging.Error);
        }

        var (resolvedPage, resolvedPageSize) = paging.Value;

        var where = new StringBuilder("""
            WHERE t.[LearningObjectiveId] = @LearningObjectiveId
              AND t.[Archived] = 0
            """);

        var fromSql = """
            FROM [ticket].[Tickets] t
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
