using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.ArchiveSprint;

public sealed record ArchiveSprintCommand(int Id) : ICommand<Result<NoValue>>;

