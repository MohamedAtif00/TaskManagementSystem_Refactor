using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Projects.UpdateProject;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Projects;

/// <summary>
/// Updates an existing project.
/// </summary>
public static class UpdateProjectEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder projects)
    {
        projects.MapPut("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Update);
        return projects;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateProjectRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateProjectCommand(id, request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(project => Results.Ok(CurriculumMapping.MapProjectDetail(project)));
    }
}
