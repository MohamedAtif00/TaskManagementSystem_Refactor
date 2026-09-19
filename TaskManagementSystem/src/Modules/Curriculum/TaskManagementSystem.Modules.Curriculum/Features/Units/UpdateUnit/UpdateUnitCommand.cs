using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.UpdateUnit;

public sealed record UpdateUnitCommand(int Id, string Name) : ICommand<Result<UnitDetailResult>>;

