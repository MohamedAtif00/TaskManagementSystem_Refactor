using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.UpdateAcademicYear;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.AcademicYears;

/// <summary>
/// Updates an existing academic year.
/// </summary>
public static class UpdateAcademicYearEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder years)
    {
        years.MapPut("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Update);
        return years;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateAcademicYearRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateAcademicYearCommand(id, request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(year => Results.Ok(CurriculumMapping.MapAcademicYearDetail(year)));
    }
}
