using Dapper;
using TaskManagementSystem.BuildingBlocks.Application.Data;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

public sealed class MemberBalanceListQueries(ISqlConnectionFactory connectionFactory)
{
    public async Task<Result<PageListResult<MemberBalanceListItemResult>>> ListPagedAsync(
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var paging = PagingValidation.Resolve(page, pageSize);
        if (paging.IsFailure)
        {
            return Result.Fail<PageListResult<MemberBalanceListItemResult>>(paging.Error);
        }

        var (resolvedPage, resolvedPageSize, skip) = paging.Value;
        const string sql = """
            SELECT
                u.[Id] AS UserId,
                u.[Code],
                u.[Name],
                eb.[AnnualLeave],
                eb.[AnnualLeaveMax],
                eb.[EmergencyLeave],
                eb.[EmergencyLeaveMax],
                eb.[SickLeave],
                eb.[Permission],
                eb.[PermissionMax],
                eb.[WorkFromHome],
                eb.[WorkFromHomeMax],
                eb.[FromNextBalanceDaysUsed],
                COUNT(*) OVER() AS TotalCount
            FROM [identity].[Users] u
            INNER JOIN [hr].[EmployeeBalances] eb ON eb.[UserId] = u.[Id]
            WHERE u.[Archived] = 0
            ORDER BY u.[Name]
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;

        using var connection = connectionFactory.GetOpenConnection();
        var rows = (await connection.QueryAsync<MemberBalanceRow>(
            new CommandDefinition(
                sql,
                new { Skip = (int)skip, Take = resolvedPageSize },
                cancellationToken: cancellationToken))).ToList();

        var totalCount = rows.FirstOrDefault()?.TotalCount ?? 0;
        var items = rows.Select(row => new MemberBalanceListItemResult(
            row.UserId,
            row.Code,
            row.Name,
            row.AnnualLeave,
            row.AnnualLeaveMax,
            row.EmergencyLeave,
            row.EmergencyLeaveMax,
            row.SickLeave,
            row.Permission,
            row.PermissionMax,
            row.WorkFromHome,
            row.WorkFromHomeMax,
            row.FromNextBalanceDaysUsed)).ToList();

        return Result.Ok(new PageListResult<MemberBalanceListItemResult>(
            items,
            resolvedPage,
            resolvedPageSize,
            totalCount));
    }

    private sealed class MemberBalanceRow
    {
        public int UserId { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public int AnnualLeave { get; init; }
        public int AnnualLeaveMax { get; init; }
        public int EmergencyLeave { get; init; }
        public int EmergencyLeaveMax { get; init; }
        public int SickLeave { get; init; }
        public int Permission { get; init; }
        public int PermissionMax { get; init; }
        public int WorkFromHome { get; init; }
        public int WorkFromHomeMax { get; init; }
        public int FromNextBalanceDaysUsed { get; init; }
        public int TotalCount { get; init; }
    }
}
