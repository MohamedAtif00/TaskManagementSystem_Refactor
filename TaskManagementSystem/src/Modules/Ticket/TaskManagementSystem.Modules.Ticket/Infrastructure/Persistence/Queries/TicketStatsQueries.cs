using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketStats;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class TicketStatsQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<TicketStatsResult> GetAsync(
        IReadOnlyList<int>? subjectIds = null,
        IReadOnlyList<int>? learningObjectiveIds = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        var ticketFilters = new StringBuilder("""
            WHERE t.[Archived] = 0
              AND lo.[Archived] = 0
            """);
        var loFilters = new StringBuilder("""
            WHERE lo.[Archived] = 0
            """);

        if (subjectIds is { Count: > 0 })
        {
            ticketFilters.Append(" AND u.[SubjectId] IN @SubjectIds");
            loFilters.Append(" AND u.[SubjectId] IN @SubjectIds");
            parameters.Add("SubjectIds", subjectIds);
        }

        if (learningObjectiveIds is { Count: > 0 })
        {
            ticketFilters.Append(" AND t.[LearningObjectiveId] IN @LearningObjectiveIds");
            loFilters.Append(" AND lo.[Id] IN @LearningObjectiveIds");
            parameters.Add("LearningObjectiveIds", learningObjectiveIds);
        }

        var ticketSql = $"""
            SELECT
                t.[Id],
                t.[Status],
                t.[UserId],
                t.[LearningObjectiveId],
                u.[SubjectId]
            FROM [ticket].[Tickets] t
            INNER JOIN [curriculum].[LearningObjectives] lo ON t.[LearningObjectiveId] = lo.[Id]
            INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
            INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
            {ticketFilters}
            """;

        var loSql = $"""
            SELECT
                lo.[Id],
                u.[SubjectId]
            FROM [curriculum].[LearningObjectives] lo
            INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
            INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
            {loFilters}
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var tickets = await connection.QueryAsync<TicketStatsItemResult>(
            new CommandDefinition(ticketSql, parameters, cancellationToken: cancellationToken));
        var learningObjectives = await connection.QueryAsync<TicketStatsLoResult>(
            new CommandDefinition(loSql, parameters, cancellationToken: cancellationToken));

        return new TicketStatsResult(tickets.ToList(), learningObjectives.ToList());
    }
}
