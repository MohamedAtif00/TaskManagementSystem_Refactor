using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Holidays;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

public sealed class HolidayListQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<Result<PageListResult<HolidayResult>>> ListPagedAsync(
        DateTime? fromDate,
        DateTime? toDate,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var paging = PagingValidation.Resolve(page, pageSize);
        if (paging.IsFailure)
        {
            return Result.Fail<PageListResult<HolidayResult>>(paging.Error);
        }

        var (resolvedPage, resolvedPageSize, skip) = paging.Value;
        var parameters = new DynamicParameters();
        parameters.Add("FromDate", fromDate?.Date);
        parameters.Add("ToDate", toDate?.Date);
        parameters.Add("Skip", (int)skip);
        parameters.Add("Take", resolvedPageSize);

        const string where = """
            WHERE (@FromDate IS NULL OR h.[EndDate] >= @FromDate)
              AND (@ToDate IS NULL OR h.[StartDate] <= @ToDate)
            """;

        using var connection = connectionFactory.GetOpenConnection();

        var countSql = $"""
            SELECT COUNT(*)
            FROM [hr].[PublicHolidays] h
            {where}
            """;
        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));

        var listSql = $"""
            SELECT
                h.[Id],
                h.[Name],
                h.[Description],
                h.[StartDate],
                h.[EndDate],
                h.[CreatedAt],
                h.[CreatedByUserId]
            FROM [hr].[PublicHolidays] h
            {where}
            ORDER BY h.[StartDate], h.[Name]
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;

        var rows = await connection.QueryAsync<HolidayRow>(
            new CommandDefinition(listSql, parameters, cancellationToken: cancellationToken));

        var items = rows.Select(row => new HolidayResult(
            row.Id,
            row.Name,
            row.Description,
            row.StartDate,
            row.EndDate,
            row.CreatedAt,
            row.CreatedByUserId)).ToList();

        return Result.Ok(new PageListResult<HolidayResult>(
            items,
            resolvedPage,
            resolvedPageSize,
            totalCount));
    }

    private sealed class HolidayRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public DateTime CreatedAt { get; init; }
        public int CreatedByUserId { get; init; }
    }
}
