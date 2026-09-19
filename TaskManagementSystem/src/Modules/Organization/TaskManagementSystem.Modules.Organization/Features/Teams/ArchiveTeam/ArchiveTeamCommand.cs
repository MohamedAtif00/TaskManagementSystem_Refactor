using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;

namespace TaskManagementSystem.Modules.Organization.Features.Teams.ArchiveTeam;

public sealed record ArchiveTeamCommand(int TeamId) : ICommand<Result<NoValue>>;

