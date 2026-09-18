using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
 using TaskManagementSystem.Modules.Curriculum.Features.Projects.ArchiveProject;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Projects;

/// <summary>
/// Archives a project.
/// </summary>
public static class ArchiveProjectEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder projects)
    {
        projects.MapDelete("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Delete);
        return projects;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveProjectCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
