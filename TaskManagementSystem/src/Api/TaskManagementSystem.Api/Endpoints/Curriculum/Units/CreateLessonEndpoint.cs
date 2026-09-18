using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Lessons.CreateLesson;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Units;

/// <summary>
/// Creates a lesson under a unit.
/// </summary>
public static class CreateLessonEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder units)
    {
        units.MapPost("/{unitId:int}/lessons", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Create);
        return units;
    }

    private static async Task<IResult> HandleAsync(
        int unitId,
        CreateLessonRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateLessonCommand(unitId, request.Name), cancellationToken);
        return result.ToHttpResult(lesson => Results.Created($"/curriculum/lessons/{lesson.Id}", CurriculumMapping.MapLessonDetail(lesson)));
    }
}
