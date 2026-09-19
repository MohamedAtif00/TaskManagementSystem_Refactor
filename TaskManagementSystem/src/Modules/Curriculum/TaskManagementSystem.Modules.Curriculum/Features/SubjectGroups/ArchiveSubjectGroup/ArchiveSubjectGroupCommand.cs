using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.ArchiveSubjectGroup;

public sealed record ArchiveSubjectGroupCommand(int Id) : ICommand<Result<NoValue>>;

