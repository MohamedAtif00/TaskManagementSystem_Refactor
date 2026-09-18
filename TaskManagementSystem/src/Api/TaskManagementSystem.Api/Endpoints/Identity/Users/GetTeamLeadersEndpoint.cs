using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Users.GetTeamLeaders;

namespace TaskManagementSystem.Api.Endpoints.Identity.Users;

/// <summary>
/// GET /identity/users/team-leaders — Lists team leaders.
/// </summary>
public static class GetTeamLeadersEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder users)
    {
        users.MapGet("/team-leaders", HandleAsync)
            .RequirePermissionCode(PermissionCodes.IdentityUsers.Read);
        return users;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeamLeadersQuery(), cancellationToken);
        return result.ToHttpResult(leaders => Results.Ok(leaders.Select(IdentityMapping.ToTeamLeaderResponse)));
    }
}
