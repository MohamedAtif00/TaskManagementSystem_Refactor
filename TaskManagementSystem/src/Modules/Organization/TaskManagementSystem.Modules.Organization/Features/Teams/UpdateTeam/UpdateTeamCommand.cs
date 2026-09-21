using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Teams.UpdateTeam;

public sealed record UpdateTeamCommand(int TeamId, string Name, int? TeamleaderId) : ICommand<Result<TeamListItemResult>>;

