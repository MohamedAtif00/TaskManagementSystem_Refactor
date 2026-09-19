using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.ArchiveProject;

public sealed record ArchiveProjectCommand(int Id) : ICommand<Result<NoValue>>;

