using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsPaged;

public sealed class ListSubjectsPagedQueryHandler(SubjectCatalogQueries queries)
    : IRequestHandler<ListSubjectsPagedQuery, Result<PageListResult<SubjectCatalogListItemResult>>>
{
    public Task<Result<PageListResult<SubjectCatalogListItemResult>>> Handle(
        ListSubjectsPagedQuery request,
        CancellationToken cancellationToken) =>
        queries.ListPagedAsync(
            request.Search,
            request.Year,
            request.Term,
            request.Page,
            request.PageSize,
            request.ActiveOnly,
            cancellationToken);
}
