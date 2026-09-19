using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.ListAcademicYears;

public sealed record ListAcademicYearsQuery : IQuery<Result<IReadOnlyList<AcademicYearListItemResult>>>;

