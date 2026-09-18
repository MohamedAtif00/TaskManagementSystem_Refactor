using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Organization.Features.Teams.ArchiveTeam;

namespace TaskManagementSystem.Api.Endpoints.Organization.Teams;

/// <summary>
/// DELETE /organization/teams/{id} — Archives a team.
/// </summary>
public static class ArchiveTeamEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder teams)
    {
        teams.MapDelete("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Organization.Delete);
        return teams;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveTeamCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
