using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.UpdateSubject;

public sealed record UpdateSubjectCommand(int Id, string Name, string Description) : ICommand<Result<SubjectDetailResult>>;

