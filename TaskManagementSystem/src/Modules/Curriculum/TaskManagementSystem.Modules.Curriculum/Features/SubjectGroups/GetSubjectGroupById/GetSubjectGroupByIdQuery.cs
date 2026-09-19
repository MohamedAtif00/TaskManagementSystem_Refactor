using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.GetSubjectGroupById;

public sealed record GetSubjectGroupByIdQuery(int Id) : IQuery<Result<SubjectGroupDetailResult>>;

