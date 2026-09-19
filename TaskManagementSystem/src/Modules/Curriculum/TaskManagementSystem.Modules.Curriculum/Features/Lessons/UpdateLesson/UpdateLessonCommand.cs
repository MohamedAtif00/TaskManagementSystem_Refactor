using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.UpdateLesson;

public sealed record UpdateLessonCommand(int Id, string Name) : ICommand<Result<LessonDetailResult>>;

