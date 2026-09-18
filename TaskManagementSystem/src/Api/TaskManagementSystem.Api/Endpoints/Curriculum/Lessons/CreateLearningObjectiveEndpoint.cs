using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.CreateLearningObjective;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Lessons;

/// <summary>
/// Creates a learning objective under a lesson.
/// </summary>
public static class CreateLearningObjectiveEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder lessons)
    {
        lessons.MapPost("/{lessonId:int}/learning-objectives", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Create);
        return lessons;
    }

    private static async Task<IResult> HandleAsync(
        int lessonId,
        CreateLearningObjectiveRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateLearningObjectiveCommand(lessonId, request.SchemaId, request.Name, request.Tag, request.Template, request.Environment),
            cancellationToken);
        return result.ToHttpResult(objective => Results.Created($"/curriculum/learning-objectives/{objective.Id}", CurriculumMapping.MapLearningObjectiveDetail(objective)));
    }
}
