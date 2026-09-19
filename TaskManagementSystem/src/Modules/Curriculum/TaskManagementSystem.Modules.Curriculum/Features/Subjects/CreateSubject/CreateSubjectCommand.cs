using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.CreateSubject;

public sealed record CreateSubjectCommand(int SubjectGroupId, string Name, string Description) : ICommand<Result<SubjectDetailResult>>;

