using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.CreateAcademicYear;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.AcademicYears;

/// <summary>
/// Creates a new academic year.
/// </summary>
public static class CreateAcademicYearEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder years)
    {
        years.MapPost("", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Create);
        return years;
    }

    private static async Task<IResult> HandleAsync(
        CreateAcademicYearRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateAcademicYearCommand(request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(year => Results.Created($"/curriculum/years/{year.Id}", CurriculumMapping.MapAcademicYearDetail(year)));
    }
}
