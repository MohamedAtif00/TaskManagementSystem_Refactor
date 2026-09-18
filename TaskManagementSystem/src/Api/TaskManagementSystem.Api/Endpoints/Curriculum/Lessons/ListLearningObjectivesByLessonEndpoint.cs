using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.ListLearningObjectivesByLesson;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Lessons;

/// <summary>
/// Lists learning objectives for a lesson.
/// </summary>
public static class ListLearningObjectivesByLessonEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder lessons)
    {
        lessons.MapGet("/{lessonId:int}/learning-objectives", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return lessons;
    }

    private static async Task<IResult> HandleAsync(
        int lessonId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListLearningObjectivesByLessonQuery(lessonId), cancellationToken);
        return result.ToHttpResult(objectives => Results.Ok(objectives.Select(CurriculumMapping.MapLearningObjectiveListItem).ToList()));
    }
}
