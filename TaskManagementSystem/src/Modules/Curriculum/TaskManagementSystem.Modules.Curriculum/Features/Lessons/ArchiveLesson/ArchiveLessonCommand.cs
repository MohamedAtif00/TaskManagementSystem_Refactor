using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.ArchiveLesson;

public sealed record ArchiveLessonCommand(int Id) : ICommand<Result<NoValue>>;

