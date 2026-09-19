using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.GetTermById;

public sealed record GetTermByIdQuery(int Id) : IQuery<Result<CurriculumTermDetailResult>>;

