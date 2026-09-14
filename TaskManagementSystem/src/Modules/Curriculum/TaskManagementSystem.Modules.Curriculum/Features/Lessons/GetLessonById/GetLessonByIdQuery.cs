using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.GetLessonById;

public sealed record GetLessonByIdQuery(int Id) : IQuery<Result<LessonDetailResult>>;

public sealed class GetLessonByIdQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<GetLessonByIdQuery, Result<LessonDetailResult>>
{
    public async Task<Result<LessonDetailResult>> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var lesson = await unitOfWork.Lessons.GetByIdAsync(request.Id, cancellationToken);
        return lesson is null ? Result.Fail<LessonDetailResult>(CurriculumErrors.LessonNotFound) : Result.Ok(LessonDetailResult.From(lesson));
    }
}
