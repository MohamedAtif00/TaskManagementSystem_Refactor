using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.GetSubjectOutline;

public sealed record GetSubjectOutlineQuery(int SubjectId) : IQuery<Result<SubjectOutlineResult>>;
