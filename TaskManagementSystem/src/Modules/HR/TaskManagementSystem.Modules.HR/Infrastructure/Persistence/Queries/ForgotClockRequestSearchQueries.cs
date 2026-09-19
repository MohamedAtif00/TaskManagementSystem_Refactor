using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

internal sealed class ForgotClockRequestSearchQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<ForgotClockRequestSearchResult> SearchAsync(
        ForgotClockRequestSearchCriteria criteria,
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

        var where = new StringBuilder("WHERE f.[Status] <> 'Cancelled' ");
        AppendRoleScope(where, parameters, criteria);

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            where.Append("AND (u.[Name] LIKE @Search OR (f.[Reason] IS NOT NULL AND f.[Reason] LIKE @Search)) ");
            parameters.Add("Search", $"%{criteria.Search}%");
        }

        if (criteria.Date.HasValue)
        {
            where.Append("AND f.[AttendanceDate] = @Date ");
            parameters.Add("Date", criteria.Date.Value.Date);
        }

        if (criteria.FromDate.HasValue)
        {
            where.Append("AND f.[AttendanceDate] >= @FromDate ");
            parameters.Add("FromDate", criteria.FromDate.Value.Date);
        }

        if (criteria.ToDate.HasValue)
        {
            where.Append("AND f.[AttendanceDate] <= @ToDate ");
            parameters.Add("ToDate", criteria.ToDate.Value.Date);
        }

        if (criteria.Status.HasValue)
        {
            where.Append("AND f.[Status] = @Status ");
            parameters.Add("Status", criteria.Status.Value.ToString());
        }

        if (criteria.PunchType.HasValue)
        {
            where.Append("AND f.[PunchType] IN @PunchTypes ");
            parameters.Add("PunchTypes", ForgotClockPunchTypeMapping.GetStorageValues(criteria.PunchType.Value));
        }

        if (!string.IsNullOrWhiteSpace(criteria.MyStatus))
        {
            AppendMyStatusFilter(where, criteria.MyStatus);
        }

        var sql = $"""
            SELECT
                f.[Id],
                f.[UserId],
                f.[PunchType],
                f.[Status],
                f.[AttendanceDate],
                f.[IntendedTime],
                f.[Reason],
                f.[CreatedAt],
                f.[UpdatedAt],
                f.[TeamleaderId],
                f.[SectionheadId],
                COUNT(*) OVER() AS TotalCount
            FROM [hr].[ForgotClockRequests] AS f
            INNER JOIN [identity].[Users] AS u ON u.[Id] = f.[UserId]
            {where}
            ORDER BY f.[CreatedAt] DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = (await connection.QueryAsync<ForgotClockRequestRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).AsList();

        if (rows.Count == 0)
        {
            return new ForgotClockRequestSearchResult([], 0);
        }

        var totalCount = rows[0].TotalCount;
        var items = rows.Select(MapToForgotClockRequest).ToList();
        return new ForgotClockRequestSearchResult(items, totalCount);
    }

    private static void AppendRoleScope(StringBuilder where, DynamicParameters parameters, ForgotClockRequestSearchCriteria criteria)
    {
        switch (criteria.ViewerRole)
        {
            case "Owner":
                return;
            case "ProjectManger":
                where.Append("AND f.[UserId] <> @ViewerUserId ");
                return;
            case "TeamLeader":
                where.Append("""
                    AND f.[UserId] <> @ViewerUserId
                    AND (u.[TeamleaderId] = @ViewerUserId OR (@ViewerTeamId IS NOT NULL AND u.[TeamId] = @ViewerTeamId))
                    """);
                return;
            case "SectionHead":
                where.Append("""
                    AND f.[UserId] <> @ViewerUserId
                    AND EXISTS (
                        SELECT 1
                        FROM [organization].[Sections] AS s
                        INNER JOIN [organization].[SectionTeams] AS st ON st.[SectionId] = s.[Id]
                        WHERE s.[HeadId] = @ViewerUserId AND st.[TeamId] = u.[TeamId]
                    )
                    """);
                return;
            default:
                where.Append("AND f.[UserId] = @ViewerUserId ");
                return;
        }
    }

    private static void AppendMyStatusFilter(StringBuilder where, string myStatus)
    {
        switch (myStatus.ToLowerInvariant())
        {
            case "pending":
                where.Append("AND f.[Status] = 'Pending' ");
                break;
            case "approved":
                where.Append("AND f.[Status] = 'Approved' ");
                break;
            case "rejected":
                where.Append("AND f.[Status] = 'Rejected' ");
                break;
        }
    }

    private static ForgotClockRequest MapToForgotClockRequest(ForgotClockRequestRow row)
    {
        var request = ForgotClockRequest.CreateForPersistence();
        request.Id = row.Id;
        request.UserId = row.UserId;
        request.PunchType = ForgotClockPunchTypeMapping.ParseFromStorage(row.PunchType);
        request.Status = Enum.Parse<ForgotClockStatus>(row.Status, ignoreCase: true);
        request.AttendanceDate = row.AttendanceDate;
        request.IntendedTime = TimeOnly.FromTimeSpan(row.IntendedTime);
        request.Reason = row.Reason;
        request.CreatedAt = row.CreatedAt;
        request.UpdatedAt = row.UpdatedAt;
        request.TeamleaderId = row.TeamleaderId;
        request.SectionheadId = row.SectionheadId;
        return request;
    }

    private sealed class ForgotClockRequestRow
    {
        public int Id { get; init; }

        public int UserId { get; init; }

        public string PunchType { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public DateTime AttendanceDate { get; init; }

        public TimeSpan IntendedTime { get; init; }

        public string? Reason { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? UpdatedAt { get; init; }

        public int? TeamleaderId { get; init; }

        public int? SectionheadId { get; init; }

        public int TotalCount { get; init; }
    }
}
