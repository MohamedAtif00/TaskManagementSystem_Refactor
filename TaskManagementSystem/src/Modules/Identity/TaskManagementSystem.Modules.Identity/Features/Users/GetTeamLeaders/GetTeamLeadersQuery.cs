using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Users.GetTeamLeaders;

public sealed record GetTeamLeadersQuery : IQuery<Result<IReadOnlyList<TeamLeaderResult>>>;
