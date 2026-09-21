using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Domain;
using TaskManagementSystem.Modules.Organization.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Organization.Features.Teams.CreateTeam;

public sealed class CreateTeamCommandHandler(
    IOrganizationUnitOfWork unitOfWork,
    IdentityLookupQueries identityLookupQueries)
    : IRequestHandler<CreateTeamCommand, Result<TeamListItemResult>>
{
    public async Task<Result<TeamListItemResult>> Handle(
        CreateTeamCommand request,
        CancellationToken cancellationToken)
    {
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

        var createResult = Team.Create(request.Name, request.TeamleaderId);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<TeamListItemResult>(createResult.Error);
        }

        await unitOfWork.Teams.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(new TeamListItemResult(
            createResult.Value.Id,
            createResult.Value.Name,
            0,
            createResult.Value.TeamleaderId,
            leaderName));
    }
}
