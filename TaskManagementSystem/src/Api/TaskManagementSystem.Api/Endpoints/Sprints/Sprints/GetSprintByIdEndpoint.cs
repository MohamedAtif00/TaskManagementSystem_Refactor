using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Sprints.Features.Sprints.GetSprintById;

namespace TaskManagementSystem.Api.Endpoints.Sprints.Sprints;

/// <summary>GET /sprints/{id} — get sprint by id. Requires Sprints.Read permission.</summary>
public static class GetSprintByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Sprints.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSprintByIdQuery(id), cancellationToken);
        return result.ToHttpResult(sprint => Results.Ok(SprintMapping.MapSprintDetail(sprint)));
    }
}
