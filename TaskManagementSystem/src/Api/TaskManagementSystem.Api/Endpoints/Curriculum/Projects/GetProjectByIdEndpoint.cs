using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Projects.GetProjectById;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Projects;

/// <summary>
/// Gets a project by identifier.
/// </summary>
public static class GetProjectByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder projects)
    {
        projects.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return projects;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProjectByIdQuery(id), cancellationToken);
        return result.ToHttpResult(project => Results.Ok(CurriculumMapping.MapProjectDetail(project)));
    }
}
