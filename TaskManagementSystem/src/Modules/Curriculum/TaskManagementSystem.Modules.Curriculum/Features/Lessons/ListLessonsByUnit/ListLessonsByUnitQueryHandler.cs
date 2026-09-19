using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.ListLessonsByUnit;

public sealed class ListLessonsByUnitQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ListLessonsByUnitQuery, Result<IReadOnlyList<LessonListItemResult>>>
{
    public async Task<Result<IReadOnlyList<LessonListItemResult>>> Handle(ListLessonsByUnitQuery request, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Lessons.ActiveUnitExistsAsync(request.UnitId, cancellationToken))
            return Result.Fail<IReadOnlyList<LessonListItemResult>>(CurriculumErrors.UnitNotFound);
        var lessons = await unitOfWork.Lessons.ListActiveByParentIdAsync(request.UnitId, cancellationToken);
        return Result.Ok<IReadOnlyList<LessonListItemResult>>(lessons.Select(LessonListItemResult.From).ToList());
    }
}

