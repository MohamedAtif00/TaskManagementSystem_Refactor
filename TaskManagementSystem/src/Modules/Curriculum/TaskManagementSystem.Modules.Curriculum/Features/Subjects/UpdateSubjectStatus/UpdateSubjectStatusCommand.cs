using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.UpdateSubjectStatus;

public sealed record UpdateSubjectStatusCommand(int SubjectId, SubjectStatus Status) : ICommand<Result<SubjectDetailResult>>;

