using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.ListLearningObjectivesByLesson;

public sealed class ListLearningObjectivesByLessonQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ListLearningObjectivesByLessonQuery, Result<IReadOnlyList<LearningObjectiveListItemResult>>>
{
    public async Task<Result<IReadOnlyList<LearningObjectiveListItemResult>>> Handle(ListLearningObjectivesByLessonQuery request, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.LearningObjectives.ActiveLessonExistsAsync(request.LessonId, cancellationToken))
            return Result.Fail<IReadOnlyList<LearningObjectiveListItemResult>>(CurriculumErrors.LessonNotFound);
        var objectives = await unitOfWork.LearningObjectives.ListActiveByParentIdAsync(request.LessonId, cancellationToken);
        return Result.Ok<IReadOnlyList<LearningObjectiveListItemResult>>(objectives.Select(LearningObjectiveListItemResult.From).ToList());
    }
}

