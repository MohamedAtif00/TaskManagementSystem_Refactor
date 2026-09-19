using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.ListTermsByProject;

public sealed record ListTermsByProjectQuery(int ProjectId) : IQuery<Result<IReadOnlyList<CurriculumTermListItemResult>>>;

