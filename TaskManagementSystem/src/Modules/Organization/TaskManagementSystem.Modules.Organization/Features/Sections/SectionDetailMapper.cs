using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Domain;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Sections;

internal static class SectionDetailMapper
{
    public static async Task<Result<SectionDetailResult>> MapAsync(
        Section section,
        IOrganizationUnitOfWork unitOfWork,
        IdentityLookupQueries identityLookupQueries,
        CancellationToken cancellationToken)
    {
        var head = await identityLookupQueries.GetActiveUserByIdAsync(section.HeadId, cancellationToken);
        if (head is null)
        {
            return Result.Fail<SectionDetailResult>(OrganizationErrors.UserNotFound);
        }

        return await MapAsync(section, head, unitOfWork, cancellationToken);
    }

    public static async Task<Result<SectionDetailResult>> MapAsync(
        Section section,
        UserSummary head,
        IOrganizationUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var teamIds = section.SectionTeams.Select(link => link.TeamId).ToArray();
        var teams = await unitOfWork.Teams.GetActiveByIdsAsync(teamIds, cancellationToken);

        var linkedTeams = teams
            .Select(team => new SectionTeamResult(team.Id, team.Name))
            .ToList();

        return Result.Ok(SectionDetailResult.From(section, head, linkedTeams));
    }
}
