using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Users.GetTeamLeaders;

public sealed record GetTeamLeadersQuery : IQuery<Result<IReadOnlyList<TeamLeaderResult>>>;

public sealed class GetTeamLeadersQueryHandler(IUserAdminQueries userAdminQueries)
    : IRequestHandler<GetTeamLeadersQuery, Result<IReadOnlyList<TeamLeaderResult>>>
{
    public async Task<Result<IReadOnlyList<TeamLeaderResult>>> Handle(
        GetTeamLeadersQuery request,
        CancellationToken cancellationToken)
    {
        var leaders = await userAdminQueries.ListTeamLeadersAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<TeamLeaderResult>>(
            leaders.Select(TeamLeaderResult.From).ToList());
    }
}
