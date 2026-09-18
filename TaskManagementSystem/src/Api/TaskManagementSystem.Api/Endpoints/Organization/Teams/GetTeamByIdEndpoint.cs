using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Organization.Features.Teams.GetTeamById;

namespace TaskManagementSystem.Api.Endpoints.Organization.Teams;

/// <summary>
/// GET /organization/teams/{id} — Gets a team by ID.
/// </summary>
public static class GetTeamByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder teams)
    {
        teams.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Organization.Read);
        return teams;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeamByIdQuery(id), cancellationToken);
        return result.ToHttpResult(team => Results.Ok(TeamMapping.MapTeamDetail(team)));
    }
}
