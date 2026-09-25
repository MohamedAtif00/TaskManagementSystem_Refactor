using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
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
