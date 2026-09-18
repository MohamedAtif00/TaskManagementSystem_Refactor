using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Lessons.UpdateLesson;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Lessons;

/// <summary>
/// Updates an existing lesson.
/// </summary>
public static class UpdateLessonEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder lessons)
    {
        lessons.MapPut("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Update);
        return lessons;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateLessonRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateLessonCommand(id, request.Name), cancellationToken);
        return result.ToHttpResult(lesson => Results.Ok(CurriculumMapping.MapLessonDetail(lesson)));
    }
}
