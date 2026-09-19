using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.UpdateSubjectGroup;

public sealed record UpdateSubjectGroupCommand(int Id, string Name) : ICommand<Result<SubjectGroupDetailResult>>;

