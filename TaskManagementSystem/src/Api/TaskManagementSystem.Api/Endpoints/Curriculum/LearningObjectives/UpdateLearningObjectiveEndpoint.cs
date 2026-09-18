using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.UpdateLearningObjective;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.LearningObjectives;

/// <summary>
/// Updates an existing learning objective.
/// </summary>
public static class UpdateLearningObjectiveEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder learningObjectives)
    {
        learningObjectives.MapPut("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Update);
        return learningObjectives;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateLearningObjectiveRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateLearningObjectiveCommand(id, request.Name, request.Tag, request.Template, request.Environment, request.StartedAt, request.DoneAt),
            cancellationToken);
        return result.ToHttpResult(objective => Results.Ok(CurriculumMapping.MapLearningObjectiveDetail(objective)));
    }
}
