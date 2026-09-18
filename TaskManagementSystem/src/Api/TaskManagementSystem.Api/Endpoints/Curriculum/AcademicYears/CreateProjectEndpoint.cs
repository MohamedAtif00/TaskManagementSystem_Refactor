using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Projects.CreateProject;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.AcademicYears;

/// <summary>
/// Creates a project under an academic year.
/// </summary>
public static class CreateProjectEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder years)
    {
        years.MapPost("/{yearId:int}/projects", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Create);
        return years;
    }

    private static async Task<IResult> HandleAsync(
        int yearId,
        CreateProjectRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateProjectCommand(yearId, request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(project => Results.Created($"/curriculum/projects/{project.Id}", CurriculumMapping.MapProjectDetail(project)));
    }
}
