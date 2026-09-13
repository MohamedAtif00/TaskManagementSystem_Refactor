using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Features.Teams;

public sealed record TeamListItemResult(int Id, string Name, int Members);

public sealed record TeamMemberResult(int Id, string Name);

public sealed record TeamDetailResult(int Id, string Name, IReadOnlyList<TeamMemberResult> Members)
{
    public static TeamDetailResult From(Team team, IReadOnlyList<UserSummary> members) =>
        new(
            team.Id,
            team.Name,
            members.Select(member => new TeamMemberResult(member.Id, member.Name)).ToList());
}
