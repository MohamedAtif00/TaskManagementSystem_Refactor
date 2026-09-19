using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsBySubjectGroup;

public sealed record ListSubjectsBySubjectGroupQuery(int SubjectGroupId) : IQuery<Result<IReadOnlyList<SubjectListItemResult>>>;

