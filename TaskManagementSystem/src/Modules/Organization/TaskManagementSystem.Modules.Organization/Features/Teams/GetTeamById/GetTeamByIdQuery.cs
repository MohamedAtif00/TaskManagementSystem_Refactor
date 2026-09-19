using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Teams.GetTeamById;

public sealed record GetTeamByIdQuery(int TeamId) : IQuery<Result<TeamDetailResult>>;

