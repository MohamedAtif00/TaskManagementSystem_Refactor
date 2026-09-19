using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.CreateUnit;

public sealed record CreateUnitCommand(int SubjectId, string Name) : ICommand<Result<UnitDetailResult>>;

