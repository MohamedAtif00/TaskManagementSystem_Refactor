using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.ListProjectsByYear;

public sealed record ListProjectsByYearQuery(int YearId) : IQuery<Result<IReadOnlyList<CurriculumProjectListItemResult>>>;

