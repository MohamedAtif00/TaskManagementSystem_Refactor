using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Terms.CreateTerm;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Projects;

/// <summary>
/// Creates a term under a project.
/// </summary>
public static class CreateTermEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder projects)
    {
        projects.MapPost("/{projectId:int}/terms", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Create);
        return projects;
    }

    private static async Task<IResult> HandleAsync(
        int projectId,
        CreateTermRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateTermCommand(projectId, request.Name, request.StartDate, request.EndDate), cancellationToken);
        return result.ToHttpResult(term => Results.Created($"/curriculum/terms/{term.Id}", CurriculumMapping.MapTermDetail(term)));
    }
}
