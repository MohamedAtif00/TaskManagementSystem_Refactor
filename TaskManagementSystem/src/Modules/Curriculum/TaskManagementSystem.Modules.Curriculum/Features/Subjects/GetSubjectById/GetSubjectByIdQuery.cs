using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.GetSubjectById;

public sealed record GetSubjectByIdQuery(int Id) : IQuery<Result<SubjectDetailResult>>;

