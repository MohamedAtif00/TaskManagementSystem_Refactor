using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.ListSprints;

public sealed class ListSprintsPagedQueryHandler(SprintListQueries queries)
    : IRequestHandler<ListSprintsPagedQuery, Result<PageListResult<SprintListItemResult>>>
{
    public Task<Result<PageListResult<SprintListItemResult>>> Handle(
        ListSprintsPagedQuery request,
        CancellationToken cancellationToken) =>
        queries.ListPagedAsync(request.Archived, request.Page, request.PageSize, cancellationToken);
}
