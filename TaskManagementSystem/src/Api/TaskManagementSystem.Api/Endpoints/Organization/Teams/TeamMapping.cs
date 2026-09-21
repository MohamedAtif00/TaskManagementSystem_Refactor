using TaskManagementSystem.Api.Contracts.Organization;
using TaskManagementSystem.Modules.Organization.Features.Teams;

namespace TaskManagementSystem.Api.Endpoints.Organization.Teams;

internal static class TeamMapping
{
    internal static TeamListItemResponse MapTeamListItem(TeamListItemResult team) =>
        new()
        {
            Id = team.Id,
            Name = team.Name,
            Members = team.Members,
            TeamleaderId = team.TeamleaderId,
            TeamleaderName = team.TeamleaderName
        };

    internal static TeamDetailResponse MapTeamDetail(TeamDetailResult team) =>
        new()
        {
            Id = team.Id,
            Name = team.Name,
            Members = team.Members
                .Select(member => new TeamMemberResponse { Id = member.Id, Name = member.Name })
                .ToList(),
            TeamleaderId = team.TeamleaderId,
            TeamleaderName = team.TeamleaderName
        };
}
