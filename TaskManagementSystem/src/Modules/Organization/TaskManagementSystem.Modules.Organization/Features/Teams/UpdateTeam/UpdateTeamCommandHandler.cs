using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Teams.UpdateTeam;

public sealed class UpdateTeamCommandHandler(
    IOrganizationUnitOfWork unitOfWork,
    IdentityLookupQueries identityLookupQueries)
    : IRequestHandler<UpdateTeamCommand, Result<TeamListItemResult>>
{
    public async Task<Result<TeamListItemResult>> Handle(
        UpdateTeamCommand request,
        CancellationToken cancellationToken)
    {
        var team = await unitOfWork.Teams.GetByIdTrackedAsync(request.TeamId, cancellationToken);
        if (team is null)
        {
            return Result.Fail<TeamListItemResult>(OrganizationErrors.TeamNotFound);
        }

        string? leaderName = null;
        if (request.TeamleaderId is int leaderId)
        {
            var leader = await identityLookupQueries.GetActiveLeaderByIdAsync(leaderId, cancellationToken);
            if (leader is null)
            {
                return Result.Fail<TeamListItemResult>(OrganizationErrors.UserNotFound);
            }

            leaderName = leader.Name;
        }

        var updateResult = team.Update(request.Name, request.TeamleaderId);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<TeamListItemResult>(updateResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        var memberCounts = await identityLookupQueries.GetMemberCountsByTeamIdsAsync(
            [team.Id],
            cancellationToken);

        return Result.Ok(new TeamListItemResult(
            team.Id,
            team.Name,
            memberCounts.GetValueOrDefault(team.Id),
            team.TeamleaderId,
            leaderName));
    }
}
