using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.ListSubjectGroupsByTerm;

public sealed record ListSubjectGroupsByTermQuery(int TermId) : IQuery<Result<IReadOnlyList<SubjectGroupListItemResult>>>;

