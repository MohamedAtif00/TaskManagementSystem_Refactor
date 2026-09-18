using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.ListAcademicYears;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.AcademicYears;

/// <summary>
/// Lists all academic years.
/// </summary>
public static class ListAcademicYearsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder years)
    {
        years.MapGet("", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return years;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListAcademicYearsQuery(), cancellationToken);
        return result.ToHttpResult(years => Results.Ok(years.Select(CurriculumMapping.MapAcademicYearListItem).ToList()));
    }
}
