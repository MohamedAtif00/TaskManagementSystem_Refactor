using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.CreateLesson;

public sealed record CreateLessonCommand(int UnitId, string Name) : ICommand<Result<LessonDetailResult>>;

