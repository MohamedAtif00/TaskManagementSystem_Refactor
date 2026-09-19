using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.UpdateLesson;

public sealed class UpdateLessonCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UpdateLessonCommand, Result<LessonDetailResult>>
{
    public async Task<Result<LessonDetailResult>> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await unitOfWork.Lessons.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (lesson is null) return Result.Fail<LessonDetailResult>(CurriculumErrors.LessonNotFound);
        var updateResult = lesson.Update(request.Name);
        if (!updateResult.IsSuccess) return Result.Fail<LessonDetailResult>(updateResult.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(LessonDetailResult.From(lesson));
    }
}

