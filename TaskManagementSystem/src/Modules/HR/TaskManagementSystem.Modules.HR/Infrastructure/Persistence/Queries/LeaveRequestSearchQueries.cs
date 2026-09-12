using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

internal sealed class LeaveRequestSearchQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<LeaveRequestSearchResult> SearchAsync(
        LeaveRequestSearchCriteria criteria,
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

        var where = new StringBuilder("WHERE lr.[Status] <> 'Cancelled' ");
        AppendRoleScope(where, parameters, criteria);

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            where.Append("AND (u.[Name] LIKE @Search OR (lr.[Reason] IS NOT NULL AND lr.[Reason] LIKE @Search)) ");
            parameters.Add("Search", $"%{criteria.Search}%");
        }

        if (criteria.FromDate.HasValue)
        {
            where.Append("AND lr.[StartDate] >= @FromDate ");
            parameters.Add("FromDate", criteria.FromDate.Value.Date);
        }

        if (criteria.ToDate.HasValue)
        {
            where.Append("AND lr.[EndDate] <= @ToDate ");
            parameters.Add("ToDate", criteria.ToDate.Value.Date);
        }

        if (criteria.Status.HasValue)
        {
            where.Append("AND lr.[Status] = @Status ");
            parameters.Add("Status", criteria.Status.Value.ToString());
        }

        if (criteria.Type.HasValue)
        {
            where.Append("AND lr.[Type] = @Type ");
            parameters.Add("Type", criteria.Type.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(criteria.MyStatus))
        {
            AppendMyStatusFilter(where, criteria.MyStatus);
        }

        var sql = $"""
            SELECT
                lr.[Id],
                lr.[UserId],
                lr.[Type],
                lr.[Status],
                lr.[StartDate],
                lr.[EndDate],
                lr.[WorkingDays],
                lr.[Reason],
                lr.[NoteForManager],
                lr.[MedicalCertificateFileName],
                lr.[MedicalCertificatePath],
                lr.[DateCreated],
                lr.[TeamleaderId],
                lr.[SectionheadId],
                COUNT(*) OVER() AS TotalCount
            FROM [hr].[LeaveRequests] AS lr
            INNER JOIN [identity].[Users] AS u ON u.[Id] = lr.[UserId]
            {where}
            ORDER BY lr.[DateCreated] DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = (await connection.QueryAsync<LeaveRequestRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).AsList();

        if (rows.Count == 0)
        {
            return new LeaveRequestSearchResult([], 0);
        }

        var totalCount = rows[0].TotalCount;
        var items = rows.Select(MapToLeaveRequest).ToList();
        return new LeaveRequestSearchResult(items, totalCount);
    }

    private static void AppendRoleScope(StringBuilder where, DynamicParameters parameters, LeaveRequestSearchCriteria criteria)
    {
        switch (criteria.ViewerRole)
        {
            case "Owner":
                return;
            case "ProjectManger":
                where.Append("AND lr.[UserId] <> @ViewerUserId ");
                return;
            case "TeamLeader":
                where.Append("""
                    AND lr.[UserId] <> @ViewerUserId
                    AND (u.[TeamleaderId] = @ViewerUserId OR (@ViewerTeamId IS NOT NULL AND u.[TeamId] = @ViewerTeamId))
                    """);
                return;
            case "SectionHead":
                where.Append("""
                    AND lr.[UserId] <> @ViewerUserId
                    AND EXISTS (
                        SELECT 1
                        FROM [organization].[Sections] AS s
                        INNER JOIN [organization].[SectionTeams] AS st ON st.[SectionId] = s.[Id]
                        WHERE s.[HeadId] = @ViewerUserId AND st.[TeamId] = u.[TeamId]
                    )
                    """);
                return;
            default:
                where.Append("AND lr.[UserId] = @ViewerUserId ");
                return;
        }
    }

    private static void AppendMyStatusFilter(StringBuilder where, string myStatus)
    {
        var normalized = myStatus.ToLowerInvariant();
        switch (normalized)
        {
            case "pending":
                where.Append("AND lr.[Status] = 'Pending' ");
                break;
            case "approved":
                where.Append("AND lr.[Status] = 'Approved' ");
                break;
            case "rejected":
                where.Append("AND lr.[Status] = 'Rejected' ");
                break;
        }
    }

    private static LeaveRequest MapToLeaveRequest(LeaveRequestRow row)
    {
        var leave = LeaveRequest.CreateForPersistence();
        leave.Id = row.Id;
        leave.UserId = row.UserId;
        leave.Type = Enum.Parse<LeaveType>(row.Type, ignoreCase: true);
        leave.Status = Enum.Parse<LeaveStatus>(row.Status, ignoreCase: true);
        leave.StartDate = row.StartDate;
        leave.EndDate = row.EndDate;
        leave.WorkingDays = row.WorkingDays;
        leave.Reason = row.Reason;
        leave.NoteForManager = row.NoteForManager;
        leave.MedicalCertificateFileName = row.MedicalCertificateFileName;
        leave.MedicalCertificatePath = row.MedicalCertificatePath;
        leave.DateCreated = row.DateCreated;
        leave.TeamleaderId = row.TeamleaderId;
        leave.SectionheadId = row.SectionheadId;
        return leave;
    }

    private sealed class LeaveRequestRow
    {
        public int Id { get; init; }

        public int UserId { get; init; }

        public string Type { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public DateTime StartDate { get; init; }

        public DateTime EndDate { get; init; }

        public int WorkingDays { get; init; }

        public string? Reason { get; init; }

        public string? NoteForManager { get; init; }

        public string? MedicalCertificateFileName { get; init; }

        public string? MedicalCertificatePath { get; init; }

        public DateTime? DateCreated { get; init; }

        public int? TeamleaderId { get; init; }

        public int? SectionheadId { get; init; }

        public int TotalCount { get; init; }
    }
}
