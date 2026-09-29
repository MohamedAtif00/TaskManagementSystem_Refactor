using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;

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
        TicketListFilter filter)
    {
        if (filter.Statuses is { Count: > 0 })
        {
            where.Append(" AND t.[Status] IN @Statuses");
            parameters.Add("Statuses", filter.Statuses.Select(status => (int)status).ToArray());
        }

        var learningObjectiveIds = filter.LearningObjectiveIds?.Where(id => id > 0).Distinct().ToArray();
        if (learningObjectiveIds is { Length: > 0 })
        {
            where.Append(" AND t.[LearningObjectiveId] IN @LearningObjectiveIds");
            parameters.Add("LearningObjectiveIds", learningObjectiveIds);
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var trimmed = filter.Name.Trim();
            if (int.TryParse(trimmed, out var ticketId) && ticketId > 0)
            {
                where.Append(" AND (t.[Name] LIKE @Name OR t.[Id] = @TicketId)");
                parameters.Add("TicketId", ticketId);
            }
            else
            {
                where.Append(" AND t.[Name] LIKE @Name");
            }

            parameters.Add("Name", $"%{trimmed}%");
        }

        var userIds = filter.UserIds?.Where(id => id > 0).Distinct().ToArray();
        if (userIds is { Length: > 0 } && filter.Unassigned)
        {
            where.Append(" AND (t.[UserId] IN @UserIds OR t.[UserId] IS NULL)");
            parameters.Add("UserIds", userIds);
        }
        else if (userIds is { Length: > 0 })
        {
            where.Append(" AND t.[UserId] IN @UserIds");
            parameters.Add("UserIds", userIds);
        }
        else if (filter.Unassigned)
        {
            where.Append(" AND t.[UserId] IS NULL");
        }

        if (filter.Priorities is { Count: > 0 })
        {
            where.Append(" AND t.[Priority] IN @Priorities");
            parameters.Add("Priorities", filter.Priorities.Select(priority => (int)priority).ToArray());
        }

        var special = new List<string>();
        if (filter.Flagged)
        {
            special.Add("t.[Flagged] = 1");
        }

        if (filter.Paused)
        {
            special.Add("t.[Pause] = 1");
        }

        if (filter.RolledBack)
        {
            special.Add("t.[IsRollback] = 1");
        }

        if (special.Count > 0)
        {
            where.Append(" AND (");
            where.Append(string.Join(" OR ", special));
            where.Append(')');
        }
    }

    public static string OrderAndPaging(bool paged) =>
        paged
            ? """
              ORDER BY t.[CreatedAt] DESC, t.[Id] DESC
              OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
              """
            : """
              ORDER BY t.[CreatedAt] DESC, t.[Id] DESC
              """;

    public static Result<(int Page, int PageSize)> AddPaging(
        DynamicParameters parameters,
        int? page,
        int? pageSize)
    {
        var resolved = PagingValidation.Resolve(page, pageSize);
        if (resolved.IsFailure)
        {
            return Result.Fail<(int Page, int PageSize)>(resolved.Error);
        }

        parameters.Add("Skip", (int)resolved.Value.Skip);
        parameters.Add("Take", resolved.Value.PageSize);
        return Result.Ok((resolved.Value.Page, resolved.Value.PageSize));
    }
}
