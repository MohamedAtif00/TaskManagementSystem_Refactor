using System.Text;
using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Domain;
using TaskManagementSystem.Modules.Curriculum.Features;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

public sealed class SubjectCatalogQueries(ISqlConnectionFactory connectionFactory)
{
    private const string BaseCte = """
        ;WITH [SubjectBase] AS (
            SELECT
                s.[Id],
                s.[Name],
                s.[Status],
                ay.[Name] AS [YearName],
                ct.[Name] AS [TermName],
                CONCAT(ay.[Name], ' / ', cp.[Name], ' / ', ct.[Name], ' / ', sg.[Name]) AS [FolderPath]
            FROM [curriculum].[Subjects] s
            INNER JOIN [curriculum].[SubjectGroups] sg ON sg.[Id] = s.[SubjectGroupId] AND sg.[Archived] = 0
            INNER JOIN [curriculum].[CurriculumTerms] ct ON ct.[Id] = sg.[TermId] AND ct.[Archived] = 0
            INNER JOIN [curriculum].[CurriculumProjects] cp ON cp.[Id] = ct.[ProjectId] AND cp.[Archived] = 0
            INNER JOIN [curriculum].[AcademicYears] ay ON ay.[Id] = cp.[YearId] AND ay.[Archived] = 0
            WHERE s.[Archived] = 0
        ),
        [LatestTickets] AS (
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
        [LoProgress] AS (
            SELECT
                u.[SubjectId],
                COUNT(*) AS [TotalLos],
                SUM(CASE WHEN lt.[Status] = 3 THEN 1 ELSE 0 END) AS [DoneLos]
            FROM [curriculum].[Units] u
            INNER JOIN [curriculum].[Lessons] l ON l.[UnitId] = u.[Id] AND l.[Archived] = 0
            INNER JOIN [curriculum].[LearningObjectives] lo ON lo.[LessonId] = l.[Id] AND lo.[Archived] = 0
            LEFT JOIN [LatestTickets] lt ON lt.[LearningObjectiveId] = lo.[Id] AND lt.[RowNum] = 1
            WHERE u.[Archived] = 0
            GROUP BY u.[SubjectId]
        ),
        [Filtered] AS (
            SELECT
                sb.[Id],
                sb.[Name],
                sb.[FolderPath],
                sb.[YearName],
                sb.[TermName],
                sb.[Status],
                CASE
                    WHEN ISNULL(lp.[TotalLos], 0) = 0 THEN 0
                    ELSE CAST(ROUND(ISNULL(lp.[DoneLos], 0) * 100.0 / lp.[TotalLos], 0) AS INT)
                END AS [ProgressPercent]
            FROM [SubjectBase] sb
            LEFT JOIN [LoProgress] lp ON lp.[SubjectId] = sb.[Id]
            WHERE (@Search IS NULL OR sb.[Name] LIKE @Search OR sb.[FolderPath] LIKE @Search)
              AND (@Year IS NULL OR sb.[YearName] = @Year)
              AND (@Term IS NULL OR sb.[TermName] = @Term)
        )
        """;

    public async Task<Result<PageListResult<SubjectCatalogListItemResult>>> ListPagedAsync(
        string? search,
        string? year,
        string? term,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var paging = PagingValidation.Resolve(page, pageSize);
        if (paging.IsFailure)
        {
            return Result.Fail<PageListResult<SubjectCatalogListItemResult>>(paging.Error);
        }

        var (resolvedPage, resolvedPageSize, skip) = paging.Value;
        var parameters = new DynamicParameters();
        parameters.Add("Search", string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%");
        parameters.Add("Year", string.IsNullOrWhiteSpace(year) ? null : year.Trim());
        parameters.Add("Term", string.IsNullOrWhiteSpace(term) ? null : term.Trim());
        parameters.Add("Skip", (int)skip);
        parameters.Add("Take", resolvedPageSize);

        using var connection = connectionFactory.GetOpenConnection();

        var countSql = BaseCte + "SELECT COUNT(*) FROM [Filtered]";
        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));

        var listSql = BaseCte + """
            SELECT [Id], [Name], [FolderPath], [YearName], [TermName], [Status], [ProgressPercent]
            FROM [Filtered]
            ORDER BY [YearName] DESC, [TermName], [Name]
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;

        var rows = await connection.QueryAsync<SubjectCatalogRow>(
            new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken));

        var items = rows.Select(row => new SubjectCatalogListItemResult(
            row.Id,
            row.Name,
            row.FolderPath,
            row.YearName,
            row.TermName,
            (SubjectStatus)row.Status,
            row.ProgressPercent)).ToList();

        return Result.Ok(new PageListResult<SubjectCatalogListItemResult>(
            items,
            resolvedPage,
            resolvedPageSize,
            totalCount));
    }

    public async Task<Result<IReadOnlyList<SubjectCatalogListItemResult>>> ListAllAsync(
        string? search,
        string? year,
        string? term,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Search", string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%");
        parameters.Add("Year", string.IsNullOrWhiteSpace(year) ? null : year.Trim());
        parameters.Add("Term", string.IsNullOrWhiteSpace(term) ? null : term.Trim());

        var listSql = BaseCte + """
            SELECT [Id], [Name], [FolderPath], [YearName], [TermName], [Status], [ProgressPercent]
            FROM [Filtered]
            ORDER BY [YearName] DESC, [TermName], [Name]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = await connection.QueryAsync<SubjectCatalogRow>(
            new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken));

        return Result.Ok<IReadOnlyList<SubjectCatalogListItemResult>>(
            rows.Select(row => new SubjectCatalogListItemResult(
                row.Id,
                row.Name,
                row.FolderPath,
                row.YearName,
                row.TermName,
                (SubjectStatus)row.Status,
                row.ProgressPercent)).ToList());
    }

    public async Task<SubjectCatalogFilterOptionsResult> GetFilterOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT DISTINCT ay.[Name] AS [YearName], ct.[Name] AS [TermName]
            FROM [curriculum].[Subjects] s
            INNER JOIN [curriculum].[SubjectGroups] sg ON sg.[Id] = s.[SubjectGroupId] AND sg.[Archived] = 0
            INNER JOIN [curriculum].[CurriculumTerms] ct ON ct.[Id] = sg.[TermId] AND ct.[Archived] = 0
            INNER JOIN [curriculum].[CurriculumProjects] cp ON cp.[Id] = ct.[ProjectId] AND cp.[Archived] = 0
            INNER JOIN [curriculum].[AcademicYears] ay ON ay.[Id] = cp.[YearId] AND ay.[Archived] = 0
            WHERE s.[Archived] = 0
            ORDER BY ay.[Name] DESC, ct.[Name]
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = await connection.QueryAsync<(string YearName, string TermName)>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        var years = rows.Select(row => row.YearName).Distinct().ToList();
        var terms = rows.Select(row => row.TermName).Distinct().OrderBy(name => name).ToList();
        return new SubjectCatalogFilterOptionsResult(years, terms);
    }

    private sealed class SubjectCatalogRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string FolderPath { get; init; } = string.Empty;
        public string YearName { get; init; } = string.Empty;
        public string TermName { get; init; } = string.Empty;
        public int Status { get; init; }
        public int ProgressPercent { get; init; }
    }
}
