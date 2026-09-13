using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

internal sealed class PermissionRequestSearchQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<PermissionRequestSearchResult> SearchAsync(
        PermissionRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, criteria.Page);
        var pageSize = Math.Clamp(criteria.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var parameters = new DynamicParameters();
        parameters.Add("Skip", skip);
        parameters.Add("Take", pageSize);
        parameters.Add("ViewerUserId", criteria.ViewerUserId);
        parameters.Add("ViewerTeamId", criteria.ViewerTeamId);

        var where = new StringBuilder("WHERE p.[Status] <> 'Cancelled' ");
        AppendRoleScope(where, parameters, criteria);

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            where.Append("AND (u.[Name] LIKE @Search OR (p.[Reason] IS NOT NULL AND p.[Reason] LIKE @Search)) ");
            parameters.Add("Search", $"%{criteria.Search}%");
        }

        if (criteria.Date.HasValue)
        {
            where.Append("AND p.[PermissionDate] = @Date ");
            parameters.Add("Date", criteria.Date.Value.Date);
        }

        if (criteria.FromDate.HasValue)
        {
            where.Append("AND p.[PermissionDate] >= @FromDate ");
            parameters.Add("FromDate", criteria.FromDate.Value.Date);
        }

        if (criteria.ToDate.HasValue)
        {
            where.Append("AND p.[PermissionDate] <= @ToDate ");
            parameters.Add("ToDate", criteria.ToDate.Value.Date);
        }

        if (criteria.Status.HasValue)
        {
            where.Append("AND p.[Status] = @Status ");
            parameters.Add("Status", criteria.Status.Value.ToString());
        }

        if (criteria.Type.HasValue)
        {
            where.Append("AND p.[Type] = @Type ");
            parameters.Add("Type", criteria.Type.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(criteria.MyStatus))
        {
            AppendMyStatusFilter(where, criteria.MyStatus);
        }

        var sql = $"""
            SELECT
                p.[Id],
                p.[UserId],
                p.[Type],
                p.[Status],
                p.[PermissionDate],
                p.[FromTime],
                p.[ToTime],
                p.[Reason],
                p.[CreatedAt],
                p.[UpdatedAt],
                p.[TeamleaderId],
                p.[SectionheadId],
                COUNT(*) OVER() AS TotalCount
            FROM [hr].[Permissions] AS p
            INNER JOIN [identity].[Users] AS u ON u.[Id] = p.[UserId]
            {where}
            ORDER BY p.[CreatedAt] DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = (await connection.QueryAsync<PermissionRequestRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).AsList();

        if (rows.Count == 0)
        {
            return new PermissionRequestSearchResult([], 0);
        }

        var totalCount = rows[0].TotalCount;
        var items = rows.Select(MapToPermissionRequest).ToList();
        return new PermissionRequestSearchResult(items, totalCount);
    }

    private static void AppendRoleScope(StringBuilder where, DynamicParameters parameters, PermissionRequestSearchCriteria criteria)
    {
        switch (criteria.ViewerRole)
        {
            case "Owner":
                return;
            case "ProjectManger":
                where.Append("AND p.[UserId] <> @ViewerUserId ");
                return;
            case "TeamLeader":
                where.Append("""
                    AND p.[UserId] <> @ViewerUserId
                    AND (u.[TeamleaderId] = @ViewerUserId OR (@ViewerTeamId IS NOT NULL AND u.[TeamId] = @ViewerTeamId))
                    """);
                return;
            case "SectionHead":
                where.Append("""
                    AND p.[UserId] <> @ViewerUserId
                    AND EXISTS (
                        SELECT 1
                        FROM [organization].[Sections] AS s
                        INNER JOIN [organization].[SectionTeams] AS st ON st.[SectionId] = s.[Id]
                        WHERE s.[HeadId] = @ViewerUserId AND st.[TeamId] = u.[TeamId]
                    )
                    """);
                return;
            default:
                where.Append("AND p.[UserId] = @ViewerUserId ");
                return;
        }
    }

    private static void AppendMyStatusFilter(StringBuilder where, string myStatus)
    {
        switch (myStatus.ToLowerInvariant())
        {
            case "pending":
                where.Append("AND p.[Status] = 'Pending' ");
                break;
            case "approved":
                where.Append("AND p.[Status] = 'Approved' ");
                break;
            case "rejected":
                where.Append("AND p.[Status] = 'Rejected' ");
                break;
        }
    }

    private static PermissionRequest MapToPermissionRequest(PermissionRequestRow row)
    {
        var permission = PermissionRequest.CreateForPersistence();
        permission.Id = row.Id;
        permission.UserId = row.UserId;
        permission.Type = Enum.Parse<PermissionType>(row.Type, ignoreCase: true);
        permission.Status = Enum.Parse<PermissionStatus>(row.Status, ignoreCase: true);
        permission.PermissionDate = row.PermissionDate;
        permission.FromTime = TimeOnly.FromTimeSpan(row.FromTime);
        permission.ToTime = TimeOnly.FromTimeSpan(row.ToTime);
        permission.Reason = row.Reason;
        permission.CreatedAt = row.CreatedAt;
        permission.UpdatedAt = row.UpdatedAt;
        permission.TeamleaderId = row.TeamleaderId;
        permission.SectionheadId = row.SectionheadId;
        return permission;
    }

    private sealed class PermissionRequestRow
    {
        public int Id { get; init; }

        public int UserId { get; init; }

        public string Type { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public DateTime PermissionDate { get; init; }

        public TimeSpan FromTime { get; init; }

        public TimeSpan ToTime { get; init; }

        public string? Reason { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? UpdatedAt { get; init; }

        public int? TeamleaderId { get; init; }

        public int? SectionheadId { get; init; }

        public int TotalCount { get; init; }
    }
}
