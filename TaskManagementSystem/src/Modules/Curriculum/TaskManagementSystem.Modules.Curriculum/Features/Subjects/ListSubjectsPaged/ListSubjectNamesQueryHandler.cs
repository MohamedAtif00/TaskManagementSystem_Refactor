using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsPaged;

public sealed class ListSubjectNamesQueryHandler(SubjectCatalogQueries queries)
    : IRequestHandler<ListSubjectNamesQuery, Result<IReadOnlyList<SubjectNameResult>>>
{
    public async Task<Result<IReadOnlyList<SubjectNameResult>>> Handle(
        ListSubjectNamesQuery request,
        CancellationToken cancellationToken)
    {
        var names = await queries.ListNamesAsync(request.ActiveOnly, cancellationToken);
        return Result.Ok(names);
    }
}
