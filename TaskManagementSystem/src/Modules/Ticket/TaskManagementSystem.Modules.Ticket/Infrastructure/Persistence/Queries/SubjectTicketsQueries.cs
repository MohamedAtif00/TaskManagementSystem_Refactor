using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.Ticket.Features;
using DomainTaskStatus = TaskManagementSystem.Modules.Ticket.Domain.TaskStatus;
using DomainTaskPriority = TaskManagementSystem.Modules.Ticket.Domain.TaskPriority;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

public sealed class SubjectTicketsQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<IReadOnlyList<TicketListItemResult>> ListBySubjectAsync(
        int subjectId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                t.[Id],
                t.[Name],
                t.[Status],
                t.[Priority],
                t.[Duration],
                t.[CreatedAt],
                t.[LearningObjectiveId],
                t.[StepId],
                t.[UserId],
                t.[TeamId]
            FROM [ticket].[Tasks] t
            INNER JOIN [curriculum].[LearningObjectives] lo ON t.[LearningObjectiveId] = lo.[Id]
            INNER JOIN [curriculum].[Lessons] l ON lo.[LessonId] = l.[Id]
            INNER JOIN [curriculum].[Units] u ON l.[UnitId] = u.[Id]
            WHERE u.[SubjectId] = @SubjectId
              AND t.[Archived] = 0
              AND lo.[Archived] = 0
            ORDER BY t.[CreatedAt] DESC
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = await connection.QueryAsync<TicketRow>(
            new CommandDefinition(sql, new { SubjectId = subjectId }, cancellationToken: cancellationToken));

        return rows.Select(TicketListItemResult.FromRow).ToList();
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
        int? TeamId);
}
