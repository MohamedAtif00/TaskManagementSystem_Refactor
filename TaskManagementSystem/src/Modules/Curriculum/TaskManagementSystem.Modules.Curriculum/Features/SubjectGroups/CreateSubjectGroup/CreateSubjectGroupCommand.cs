using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.CreateSubjectGroup;

public sealed record CreateSubjectGroupCommand(int TermId, string Name) : ICommand<Result<SubjectGroupDetailResult>>;

