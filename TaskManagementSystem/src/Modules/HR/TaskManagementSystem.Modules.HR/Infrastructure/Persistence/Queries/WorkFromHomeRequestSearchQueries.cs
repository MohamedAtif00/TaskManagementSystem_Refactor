using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

internal sealed class WorkFromHomeRequestSearchQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<WorkFromHomeRequestSearchResult> SearchAsync(
        WorkFromHomeRequestSearchCriteria criteria,
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

        var where = new StringBuilder("WHERE wfh.[Status] <> 3 ");
        AppendRoleScope(where, parameters, criteria);

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            where.Append("AND (u.[Name] LIKE @Search OR (wfh.[NoteForManager] IS NOT NULL AND wfh.[NoteForManager] LIKE @Search)) ");
            parameters.Add("Search", $"%{criteria.Search}%");
        }

        if (criteria.FromDate.HasValue)
        {
            where.Append("AND wfh.[Date] >= @FromDate ");
            parameters.Add("FromDate", criteria.FromDate.Value.Date);
        }

        if (criteria.ToDate.HasValue)
        {
            where.Append("AND wfh.[Date] <= @ToDate ");
            parameters.Add("ToDate", criteria.ToDate.Value.Date);
        }

        if (criteria.Status.HasValue)
        {
            where.Append("AND wfh.[Status] = @Status ");
            parameters.Add("Status", (int)criteria.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.MyStatus))
        {
            AppendMyStatusFilter(where, criteria.MyStatus);
        }

        var sql = $"""
            SELECT
                wfh.[Id],
                wfh.[UserId],
                wfh.[Date],
                wfh.[DateCreated],
                wfh.[NoteForManager],
                wfh.[Status],
                wfh.[TeamleaderId],
                wfh.[SectionheadId],
                COUNT(*) OVER() AS TotalCount
            FROM [hr].[WorkFromHomeRequests] AS wfh
            INNER JOIN [identity].[Users] AS u ON u.[Id] = wfh.[UserId]
            {where}
            ORDER BY wfh.[DateCreated] DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = (await connection.QueryAsync<WorkFromHomeRequestRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).AsList();

        if (rows.Count == 0)
        {
            return new WorkFromHomeRequestSearchResult([], 0);
        }

        var totalCount = rows[0].TotalCount;
        var items = rows.Select(MapToWorkFromHomeRequest).ToList();
        return new WorkFromHomeRequestSearchResult(items, totalCount);
    }

    private static void AppendRoleScope(StringBuilder where, DynamicParameters parameters, WorkFromHomeRequestSearchCriteria criteria)
    {
        switch (criteria.ViewerRole)
        {
            case "Owner":
                return;
            case "ProjectManger":
                where.Append("AND wfh.[UserId] <> @ViewerUserId ");
                return;
            case "TeamLeader":
                where.Append("""
                    AND wfh.[UserId] <> @ViewerUserId
                    AND (u.[TeamleaderId] = @ViewerUserId OR (@ViewerTeamId IS NOT NULL AND u.[TeamId] = @ViewerTeamId))
                    """);
                return;
            case "SectionHead":
                where.Append("""
                    AND wfh.[UserId] <> @ViewerUserId
                    AND EXISTS (
                        SELECT 1
                        FROM [organization].[Sections] AS s
                        INNER JOIN [organization].[SectionTeams] AS st ON st.[SectionId] = s.[Id]
                        WHERE s.[HeadId] = @ViewerUserId AND st.[TeamId] = u.[TeamId]
                    )
                    """);
                return;
            default:
                where.Append("AND wfh.[UserId] = @ViewerUserId ");
                return;
        }
    }

    private static void AppendMyStatusFilter(StringBuilder where, string myStatus)
    {
        switch (myStatus.ToLowerInvariant())
        {
            case "pending":
                where.Append("AND wfh.[Status] = 0 ");
                break;
            case "approved":
                where.Append("AND wfh.[Status] = 1 ");
                break;
            case "rejected":
                where.Append("AND wfh.[Status] = 2 ");
                break;
        }
    }

    private static WorkFromHomeRequest MapToWorkFromHomeRequest(WorkFromHomeRequestRow row)
    {
        var request = WorkFromHomeRequest.CreateForPersistence();
        request.Id = row.Id;
        request.UserId = row.UserId;
        request.Date = row.Date;
        request.DateCreated = row.DateCreated;
        request.NoteForManager = row.NoteForManager;
        request.Status = (WorkFromHomeStatus)row.Status;
        request.TeamleaderId = row.TeamleaderId;
        request.SectionheadId = row.SectionheadId;
        return request;
    }

    private sealed class WorkFromHomeRequestRow
    {
        public int Id { get; init; }

        public int UserId { get; init; }

        public DateTime Date { get; init; }

        public DateTime? DateCreated { get; init; }

        public string? NoteForManager { get; init; }

        public int Status { get; init; }

        public int? TeamleaderId { get; init; }

        public int? SectionheadId { get; init; }

        public int TotalCount { get; init; }
    }
}
