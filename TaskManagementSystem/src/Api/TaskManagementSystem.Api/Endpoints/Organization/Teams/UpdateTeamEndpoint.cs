using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Organization;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Organization.Features.Teams.UpdateTeam;

namespace TaskManagementSystem.Api.Endpoints.Organization.Teams;

/// <summary>
/// PUT /organization/teams/{id} — Updates a team.
/// </summary>
public static class UpdateTeamEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder teams)
    {
        teams.MapPut("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Organization.Update);
        return teams;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateTeamRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateTeamCommand(id, request.Name), cancellationToken);
        return result.ToHttpResult(team => Results.Ok(TeamMapping.MapTeamListItem(team)));
    }
}
