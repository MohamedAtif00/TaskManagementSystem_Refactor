using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Features.Leave.ListMemberBalances;

public sealed class ListMemberBalancesQueryHandler(MemberBalanceListQueries queries)
    : IRequestHandler<ListMemberBalancesQuery, Result<PageListResult<MemberBalanceListItemResult>>>
{
    public Task<Result<PageListResult<MemberBalanceListItemResult>>> Handle(
        ListMemberBalancesQuery request,
        CancellationToken cancellationToken) =>
        queries.ListPagedAsync(request.Page, request.PageSize, cancellationToken);
}
