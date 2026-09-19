using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.ArchiveTerm;

public sealed record ArchiveTermCommand(int Id) : ICommand<Result<NoValue>>;

