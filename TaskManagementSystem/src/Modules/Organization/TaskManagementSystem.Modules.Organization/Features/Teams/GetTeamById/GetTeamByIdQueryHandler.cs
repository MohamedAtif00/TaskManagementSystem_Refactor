using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Teams.GetTeamById;

public sealed class GetTeamByIdQueryHandler(
    IOrganizationUnitOfWork unitOfWork,
    IdentityLookupQueries identityLookupQueries)
    : IRequestHandler<GetTeamByIdQuery, Result<TeamDetailResult>>
{
    public async Task<Result<TeamDetailResult>> Handle(
        GetTeamByIdQuery request,
        CancellationToken cancellationToken)
    {
        var team = await unitOfWork.Teams.GetByIdAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            return Result.Fail<TeamDetailResult>(OrganizationErrors.TeamNotFound);
        }

        var members = await identityLookupQueries.GetActiveTeamMembersAsync(team.Id, cancellationToken);
        string? leaderName = null;
        if (team.TeamleaderId is int leaderId)
        {
            var leaders = await identityLookupQueries.GetActiveUsersByIdsAsync([leaderId], cancellationToken);
            leaderName = leaders.GetValueOrDefault(leaderId)?.Name;
        }

        return Result.Ok(TeamDetailResult.From(team, members, leaderName));
    }
}
