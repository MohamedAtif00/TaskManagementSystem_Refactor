using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.GetAcademicYearById;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.AcademicYears;

/// <summary>
/// Gets an academic year by identifier.
/// </summary>
public static class GetAcademicYearByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder years)
    {
        years.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return years;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAcademicYearByIdQuery(id), cancellationToken);
        return result.ToHttpResult(year => Results.Ok(CurriculumMapping.MapAcademicYearDetail(year)));
    }
}
