using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Organization;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Organization.Features.Teams.CreateTeam;

namespace TaskManagementSystem.Api.Endpoints.Organization.Teams;

/// <summary>
/// POST /organization/teams — Creates a new team.
/// </summary>
public static class CreateTeamEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder teams)
    {
        teams.MapPost("", HandleAsync).RequirePermissionCode(PermissionCodes.Organization.Create);
        return teams;
    }

    private static async Task<IResult> HandleAsync(
        CreateTeamRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateTeamCommand(request.Name), cancellationToken);
        return result.ToHttpResult(team =>
            Results.Created($"/organization/teams/{team.Id}", TeamMapping.MapTeamListItem(team)));
    }
}
