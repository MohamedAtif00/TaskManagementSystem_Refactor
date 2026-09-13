using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Features.Sections;

public sealed record SectionListItemResult(int Id, string Name);

public sealed record SectionTeamResult(int Id, string Name);

public sealed record SectionDetailResult(
    int Id,
    string Name,
    UserSummary Head,
    IReadOnlyList<SectionTeamResult> Teams)
{
    public static SectionDetailResult From(
        Section section,
        UserSummary head,
        IReadOnlyList<SectionTeamResult> teams) =>
        new(section.Id, section.Name, head, teams);
}
