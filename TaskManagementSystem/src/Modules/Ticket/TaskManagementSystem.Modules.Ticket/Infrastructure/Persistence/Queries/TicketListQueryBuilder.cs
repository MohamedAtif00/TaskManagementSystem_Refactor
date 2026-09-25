using System.Text;
using Dapper;
using DomainTicketStatus = TaskManagementSystem.Modules.Ticket.Domain.TicketStatus;

namespace TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

internal static class TicketListQueryBuilder
{
    public const string SelectList = """
        t.[Id],
        t.[Name],
        t.[Status],
        t.[Priority],
        t.[Duration],
        t.[CreatedAt],
        t.[LearningObjectiveId],
        t.[StepId],
        t.[UserId],
        t.[TeamId],
        t.[Pause],
        t.[Attention],
        t.[Flagged],
        t.[IsRollback],
        t.[RollbackCount]
        """;

    public static void AppendFilters(
        StringBuilder where,
        DynamicParameters parameters,
        IReadOnlyList<DomainTicketStatus>? statuses,
        int? learningObjectiveId,
        string? name)
    {
        if (statuses is { Count: > 0 })
        {
            where.Append(" AND t.[Status] IN @Statuses");
            parameters.Add("Statuses", statuses.Select(status => (int)status).ToArray());
        }

        if (learningObjectiveId is > 0)
        {
            where.Append(" AND t.[LearningObjectiveId] = @LearningObjectiveId");
            parameters.Add("LearningObjectiveId", learningObjectiveId.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            where.Append(" AND t.[Name] LIKE @Name");
            parameters.Add("Name", $"%{name.Trim()}%");
        }
    }

    public static string OrderAndPaging(bool paged) =>
        paged
            ? """
              ORDER BY t.[CreatedAt] DESC
              OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
              """
            : "ORDER BY t.[CreatedAt] DESC";

    public static void AddPaging(
        DynamicParameters parameters,
        int? page,
        int? pageSize,
        out bool paged,
        out int resolvedPage,
        out int resolvedPageSize)
    {
        resolvedPage = page is >= 1 ? page.Value : 1;
        resolvedPageSize = Math.Clamp(pageSize ?? 20, 1, 5000);
        paged = true;
        parameters.Add("Skip", (resolvedPage - 1) * resolvedPageSize);
        parameters.Add("Take", resolvedPageSize);
    }
}
