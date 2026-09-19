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

        var updateResult = team.Update(request.Name);
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
            memberCounts.GetValueOrDefault(team.Id)));
    }
}

