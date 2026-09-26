using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsPaged;

public sealed class ListSubjectsExportQueryHandler(SubjectCatalogQueries queries)
    : IRequestHandler<ListSubjectsExportQuery, Result<IReadOnlyList<SubjectCatalogListItemResult>>>
{
    public Task<Result<IReadOnlyList<SubjectCatalogListItemResult>>> Handle(
        ListSubjectsExportQuery request,
        CancellationToken cancellationToken) =>
        queries.ListAllAsync(request.Search, request.Year, request.Term, request.ActiveOnly, cancellationToken);
}
