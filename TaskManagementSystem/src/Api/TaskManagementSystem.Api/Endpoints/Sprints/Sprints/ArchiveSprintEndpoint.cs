using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Sprints.Features.Sprints.ArchiveSprint;

namespace TaskManagementSystem.Api.Endpoints.Sprints.Sprints;

/// <summary>DELETE /sprints/{id} — archive a sprint. Requires Sprints.Delete permission.</summary>
public static class ArchiveSprintEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Sprints.Delete);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveSprintCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
