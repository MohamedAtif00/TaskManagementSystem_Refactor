using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.UpdateTerm;

public sealed record UpdateTermCommand(int Id, string Name, DateTime? StartDate, DateTime? EndDate) : ICommand<Result<CurriculumTermDetailResult>>;

