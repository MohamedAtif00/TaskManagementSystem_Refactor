using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Curriculum.Features;
using TaskManagementSystem.Modules.Curriculum.Features.Subjects.GetSubjectOutline;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;

/// <summary>
/// Returns the units, lessons, and learning objectives for a subject in one response.
/// </summary>
public static class GetSubjectOutlineEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjects)
    {
        subjects.MapGet("/{subjectId:int}/outline", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return subjects;
    }

    private static async Task<IResult> HandleAsync(
        int subjectId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSubjectOutlineQuery(subjectId), cancellationToken);
        return result.ToHttpResult(outline => Results.Ok(Map(outline)));
    }

    private static SubjectOutlineResponse Map(SubjectOutlineResult outline) =>
        new()
        {
            Units = outline.Units.Select(unit => new SubjectOutlineUnitResponse
            {
                Id = unit.Id,
                Name = unit.Name,
                Lessons = unit.Lessons.Select(lesson => new SubjectOutlineLessonResponse
                {
                    Id = lesson.Id,
                    Name = lesson.Name,
                    LearningObjectives = lesson.LearningObjectives.Select(objective => new SubjectOutlineLearningObjectiveResponse
                    {
                        Id = objective.Id,
                        Name = objective.Name,
                        Tag = objective.Tag,
                        Template = objective.Template,
                        Environment = objective.Environment,
                        LessonId = objective.LessonId
                    }).ToList()
                }).ToList()
            }).ToList()
        };
}
