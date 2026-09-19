using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.ArchiveUnit;

public sealed record ArchiveUnitCommand(int Id) : ICommand<Result<NoValue>>;

