using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Features;

namespace TaskManagementSystem.Modules.Sprints.Infrastructure.Persistence.Queries;

public sealed class SprintListQueries(ISqlConnectionFactory connectionFactory)
{
    private const string BaseCte = """
        ;WITH [LatestTickets] AS (
            SELECT
                t.[LearningObjectiveId],
                t.[Status],
                ROW_NUMBER() OVER (
                    PARTITION BY t.[LearningObjectiveId]
                    ORDER BY t.[CreatedAt] DESC, t.[Id] DESC
                ) AS [RowNum]
            FROM [ticket].[Tickets] t
            WHERE t.[Archived] = 0
        ),
        [SprintLoProgress] AS (
            SELECT
                slo.[SprintId],
                COUNT(*) AS [LearningObjectiveCount],
                SUM(CASE WHEN lt.[Status] = 3 THEN 1 ELSE 0 END) AS [DoneLos]
            FROM [sprints].[SprintLearningObjectives] slo
            INNER JOIN [curriculum].[LearningObjectives] lo ON slo.[LearningObjectiveId] = lo.[Id] AND lo.[Archived] = 0
            LEFT JOIN [LatestTickets] lt ON lt.[LearningObjectiveId] = lo.[Id] AND lt.[RowNum] = 1
            GROUP BY slo.[SprintId]
        ),
        [Filtered] AS (
            SELECT
                s.[Id],
                s.[Name],
                s.[Description],
                s.[StartDate],
                s.[EndDate],
                ISNULL(p.[LearningObjectiveCount], 0) AS [LearningObjectiveCount],
                CASE
                    WHEN ISNULL(p.[LearningObjectiveCount], 0) = 0 THEN 0
                    ELSE CAST(ROUND(ISNULL(p.[DoneLos], 0) * 100.0 / p.[LearningObjectiveCount], 0) AS INT)
                END AS [ProgressPercent]
            FROM [sprints].[Sprints] s
            LEFT JOIN [SprintLoProgress] p ON p.[SprintId] = s.[Id]
            WHERE (@Archived IS NULL AND s.[IsArchived] = 0)
               OR (@Archived IS NOT NULL AND s.[IsArchived] = @Archived)
        )
        """;

    public async Task<Result<PageListResult<SprintListItemResult>>> ListPagedAsync(
        bool? archived,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var paging = PagingValidation.Resolve(page, pageSize);
        if (paging.IsFailure)
        {
            return Result.Fail<PageListResult<SprintListItemResult>>(paging.Error);
        }

        var (resolvedPage, resolvedPageSize, skip) = paging.Value;
        var parameters = new DynamicParameters();
        parameters.Add("Archived", archived);
        parameters.Add("Skip", (int)skip);
        parameters.Add("Take", resolvedPageSize);

        using var connection = connectionFactory.GetOpenConnection();

        var countSql = BaseCte + "SELECT COUNT(*) FROM [Filtered]";
        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));

        var listSql = BaseCte + """
            SELECT [Id], [Name], [Description], [StartDate], [EndDate], [LearningObjectiveCount], [ProgressPercent]
            FROM [Filtered]
            ORDER BY [StartDate] DESC, [Id] DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;

        var rows = await connection.QueryAsync<SprintListRow>(
            new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken));

        var items = rows.Select(row => new SprintListItemResult(
            row.Id,
            row.Name,
            row.Description ?? string.Empty,
            row.StartDate,
            row.EndDate,
            row.LearningObjectiveCount,
            row.ProgressPercent)).ToList();

        return Result.Ok(new PageListResult<SprintListItemResult>(
            items,
            resolvedPage,
            resolvedPageSize,
            totalCount));
    }

    public async Task<IReadOnlyList<SprintListItemResult>> ListAllAsync(
        bool? archived,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Archived", archived);

        var listSql = BaseCte + """
            SELECT [Id], [Name], [Description], [StartDate], [EndDate], [LearningObjectiveCount], [ProgressPercent]
            FROM [Filtered]
            ORDER BY [StartDate] DESC, [Id] DESC
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = await connection.QueryAsync<SprintListRow>(
            new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken));

        return rows.Select(row => new SprintListItemResult(
            row.Id,
            row.Name,
            row.Description ?? string.Empty,
            row.StartDate,
            row.EndDate,
            row.LearningObjectiveCount,
            row.ProgressPercent)).ToList();
    }

    private sealed class SprintListRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public int LearningObjectiveCount { get; init; }
        public int ProgressPercent { get; init; }
    }
}
