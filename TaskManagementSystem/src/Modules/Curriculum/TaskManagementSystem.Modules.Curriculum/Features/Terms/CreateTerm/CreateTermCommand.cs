using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.CreateTerm;

public sealed record CreateTermCommand(int ProjectId, string Name, DateTime? StartDate, DateTime? EndDate) : ICommand<Result<CurriculumTermDetailResult>>;

