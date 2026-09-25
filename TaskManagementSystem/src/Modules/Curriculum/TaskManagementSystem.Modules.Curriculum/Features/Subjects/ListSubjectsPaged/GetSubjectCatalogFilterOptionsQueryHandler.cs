using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsPaged;

public sealed class GetSubjectCatalogFilterOptionsQueryHandler(SubjectCatalogQueries queries)
    : IRequestHandler<GetSubjectCatalogFilterOptionsQuery, Result<SubjectCatalogFilterOptionsResult>>
{
    public async Task<Result<SubjectCatalogFilterOptionsResult>> Handle(
        GetSubjectCatalogFilterOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var options = await queries.GetFilterOptionsAsync(cancellationToken);
        return Result.Ok(options);
    }
}
