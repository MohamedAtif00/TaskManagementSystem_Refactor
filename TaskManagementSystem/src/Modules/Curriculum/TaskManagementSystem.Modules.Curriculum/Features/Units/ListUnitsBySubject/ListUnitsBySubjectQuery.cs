using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.ListUnitsBySubject;

public sealed record ListUnitsBySubjectQuery(int SubjectId) : IQuery<Result<IReadOnlyList<UnitListItemResult>>>;

