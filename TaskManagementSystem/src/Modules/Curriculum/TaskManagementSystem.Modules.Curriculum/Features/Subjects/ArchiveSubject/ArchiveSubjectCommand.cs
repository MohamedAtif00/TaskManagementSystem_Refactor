using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ArchiveSubject;

public sealed record ArchiveSubjectCommand(int Id) : ICommand<Result<NoValue>>;

