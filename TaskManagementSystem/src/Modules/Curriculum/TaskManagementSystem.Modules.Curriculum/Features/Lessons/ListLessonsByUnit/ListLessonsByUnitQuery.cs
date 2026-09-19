using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.ListLessonsByUnit;

public sealed record ListLessonsByUnitQuery(int UnitId) : IQuery<Result<IReadOnlyList<LessonListItemResult>>>;

