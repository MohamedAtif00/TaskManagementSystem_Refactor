using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.ListSprintLearningObjectives;

namespace TaskManagementSystem.Api.Endpoints.Sprints.SprintLearningObjectives;

/// <summary>GET /sprints/{id}/learning-objectives — list learning objectives for a sprint. Requires Sprints.Read permission.</summary>
public static class ListSprintLearningObjectivesEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}/learning-objectives", HandleAsync).RequirePermissionCode(PermissionCodes.Sprints.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSprintLearningObjectivesQuery(id), cancellationToken);
        return result.ToHttpResult(learningObjectiveIds => Results.Ok(learningObjectiveIds));
    }
}
