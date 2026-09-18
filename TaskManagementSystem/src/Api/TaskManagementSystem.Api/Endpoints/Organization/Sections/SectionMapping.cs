using TaskManagementSystem.Api.Contracts.Organization;
using TaskManagementSystem.Modules.Organization.Features.Sections;

namespace TaskManagementSystem.Api.Endpoints.Organization.Sections;

internal static class SectionMapping
{
    internal static SectionListItemResponse MapSectionListItem(SectionListItemResult section) =>
        new()
        {
            Id = section.Id,
            Name = section.Name
        };

    internal static SectionDetailResponse MapSectionDetail(SectionDetailResult section) =>
        new()
        {
            Id = section.Id,
            Name = section.Name,
            Head = new IdNameResponse { Id = section.Head.Id, Name = section.Head.Name },
            Teams = section.Teams
                .Select(team => new SectionTeamResponse { Id = team.Id, Name = team.Name })
                .ToList()
        };
}
