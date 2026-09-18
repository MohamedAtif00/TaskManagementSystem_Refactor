using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Units.ListUnitsBySubject;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;

/// <summary>
/// Lists units for a subject.
/// </summary>
public static class ListUnitsBySubjectEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjects)
    {
        subjects.MapGet("/{subjectId:int}/units", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return subjects;
    }

    private static async Task<IResult> HandleAsync(
        int subjectId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListUnitsBySubjectQuery(subjectId), cancellationToken);
        return result.ToHttpResult(units => Results.Ok(units.Select(CurriculumMapping.MapUnitListItem).ToList()));
    }
}
