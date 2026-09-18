using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Projects.ListProjectsByYear;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.AcademicYears;

/// <summary>
/// Lists projects for an academic year.
/// </summary>
public static class ListProjectsByYearEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder years)
    {
        years.MapGet("/{yearId:int}/projects", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return years;
    }

    private static async Task<IResult> HandleAsync(
        int yearId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListProjectsByYearQuery(yearId), cancellationToken);
        return result.ToHttpResult(projects => Results.Ok(projects.Select(CurriculumMapping.MapProjectListItem).ToList()));
    }
}
