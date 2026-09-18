using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Lessons.ListLessonsByUnit;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Units;

/// <summary>
/// Lists lessons for a unit.
/// </summary>
public static class ListLessonsByUnitEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder units)
    {
        units.MapGet("/{unitId:int}/lessons", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return units;
    }

    private static async Task<IResult> HandleAsync(
        int unitId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListLessonsByUnitQuery(unitId), cancellationToken);
        return result.ToHttpResult(lessons => Results.Ok(lessons.Select(CurriculumMapping.MapLessonListItem).ToList()));
    }
}
