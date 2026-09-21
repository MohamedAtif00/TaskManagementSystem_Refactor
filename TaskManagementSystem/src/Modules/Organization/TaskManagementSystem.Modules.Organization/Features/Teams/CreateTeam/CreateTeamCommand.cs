using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Features.Teams.CreateTeam;

public sealed record CreateTeamCommand(string Name, int? TeamleaderId) : ICommand<Result<TeamListItemResult>>;

