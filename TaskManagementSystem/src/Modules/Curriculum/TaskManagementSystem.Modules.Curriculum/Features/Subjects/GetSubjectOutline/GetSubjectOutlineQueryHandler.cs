using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.GetSubjectOutline;

public sealed class GetSubjectOutlineQueryHandler(SubjectOutlineQueries subjectOutlineQueries)
    : IRequestHandler<GetSubjectOutlineQuery, Result<SubjectOutlineResult>>
{
    public async Task<Result<SubjectOutlineResult>> Handle(
        GetSubjectOutlineQuery request,
        CancellationToken cancellationToken)
    {
        var outline = await subjectOutlineQueries.GetBySubjectAsync(request.SubjectId, cancellationToken);
        return outline is null
            ? Result.Fail<SubjectOutlineResult>(CurriculumErrors.SubjectNotFound)
            : Result.Ok(outline);
    }
}
