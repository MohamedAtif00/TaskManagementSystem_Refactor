using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketStats;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class TicketStatsQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<TicketStatsResult> GetAsync(CancellationToken cancellationToken = default)
    {
        const string ticketSql = """
            SELECT
                t.[Id],
                t.[Status],
                t.[UserId],
                t.[LearningObjectiveId],
                u.[SubjectId]
            FROM [ticket].[Tasks] t
            INNER JOIN [curriculum].[LearningObjectives] lo ON t.[LearningObjectiveId] = lo.[Id]
            INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
            INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
            WHERE t.[Archived] = 0
              AND lo.[Archived] = 0
            """;

        const string loSql = """
            SELECT
                lo.[Id],
                u.[SubjectId]
            FROM [curriculum].[LearningObjectives] lo
            INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
            INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
            WHERE lo.[Archived] = 0
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var tickets = await connection.QueryAsync<TicketStatsItemResult>(
            new CommandDefinition(ticketSql, cancellationToken: cancellationToken));
        var learningObjectives = await connection.QueryAsync<TicketStatsLoResult>(
            new CommandDefinition(loSql, cancellationToken: cancellationToken));

        return new TicketStatsResult(tickets.ToList(), learningObjectives.ToList());
    }
}
