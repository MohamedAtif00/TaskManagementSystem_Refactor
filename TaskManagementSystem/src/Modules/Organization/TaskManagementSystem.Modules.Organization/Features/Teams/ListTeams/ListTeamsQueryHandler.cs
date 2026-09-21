using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Teams.ListTeams;

public sealed class ListTeamsQueryHandler(
    IOrganizationUnitOfWork unitOfWork,
    IdentityLookupQueries identityLookupQueries)
    : IRequestHandler<ListTeamsQuery, Result<IReadOnlyList<TeamListItemResult>>>
{
    public async Task<Result<IReadOnlyList<TeamListItemResult>>> Handle(
        ListTeamsQuery request,
        CancellationToken cancellationToken)
    {
        var teams = await unitOfWork.Teams.ListActiveAsync(cancellationToken);
        var memberCounts = await identityLookupQueries.GetMemberCountsByTeamIdsAsync(
            teams.Select(team => team.Id).ToArray(),
            cancellationToken);
        var leaderIds = teams
            .Select(team => team.TeamleaderId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToArray();
        var leaders = await identityLookupQueries.GetActiveUsersByIdsAsync(leaderIds, cancellationToken);

        var results = teams
            .Select(team => new TeamListItemResult(
                team.Id,
                team.Name,
                memberCounts.GetValueOrDefault(team.Id),
                team.TeamleaderId,
                team.TeamleaderId is int leaderId ? leaders.GetValueOrDefault(leaderId)?.Name : null))
            .ToList();

        return Result.Ok<IReadOnlyList<TeamListItemResult>>(results);
    }
}
