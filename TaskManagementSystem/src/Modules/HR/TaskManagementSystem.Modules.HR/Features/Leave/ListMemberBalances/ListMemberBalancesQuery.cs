using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.ListMemberBalances;

public sealed record ListMemberBalancesQuery(int? Page, int? PageSize)
    : IQuery<Result<PageListResult<MemberBalanceListItemResult>>>;
