using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Organization.Features.Teams.ListTeams;

namespace TaskManagementSystem.Api.Endpoints.Organization.Teams;

/// <summary>
/// GET /organization/teams — Lists all teams.
/// </summary>
public static class ListTeamsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder teams)
    {
        teams.MapGet("", HandleAsync).RequirePermissionCode(PermissionCodes.Organization.Read);
        return teams;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTeamsQuery(), cancellationToken);
        return result.ToHttpResult(teams =>
            Results.Ok(teams.Select(TeamMapping.MapTeamListItem).ToList()));
    }
}
