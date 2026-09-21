using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Sprints.Features.Sprints.ListSprints;

namespace TaskManagementSystem.Api.Endpoints.Sprints.Sprints;

/// <summary>GET /sprints — list sprints. Requires Sprints.Read permission.</summary>
public static class ListSprintsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("", HandleAsync).RequirePermissionCode(PermissionCodes.Sprints.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken,
        string? archived = null)
    {
        var result = await mediator.Send(
            new ListSprintsQuery(OptionalQueryBinding.ParseOptionalBool(archived)),
            cancellationToken);
        return result.ToHttpResult(sprints =>
            Results.Ok(sprints.Select(SprintMapping.MapSprintListItem).ToList()));
    }
}
