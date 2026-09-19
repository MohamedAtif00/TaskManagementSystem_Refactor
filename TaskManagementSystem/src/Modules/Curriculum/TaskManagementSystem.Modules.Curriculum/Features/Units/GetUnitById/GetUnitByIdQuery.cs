using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.GetUnitById;

public sealed record GetUnitByIdQuery(int Id) : IQuery<Result<UnitDetailResult>>;

