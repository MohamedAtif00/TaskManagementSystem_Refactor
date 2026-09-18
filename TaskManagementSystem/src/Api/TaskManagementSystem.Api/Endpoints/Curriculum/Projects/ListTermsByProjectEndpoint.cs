using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Terms.ListTermsByProject;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Projects;

/// <summary>
/// Lists terms for a project.
/// </summary>
public static class ListTermsByProjectEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder projects)
    {
        projects.MapGet("/{projectId:int}/terms", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return projects;
    }

    private static async Task<IResult> HandleAsync(
        int projectId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTermsByProjectQuery(projectId), cancellationToken);
        return result.ToHttpResult(terms => Results.Ok(terms.Select(CurriculumMapping.MapTermListItem).ToList()));
    }
}
