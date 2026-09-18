using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.RemoveSprintLearningObjective;

namespace TaskManagementSystem.Api.Endpoints.Sprints.SprintLearningObjectives;

/// <summary>DELETE /sprints/{id}/learning-objectives/{loId} — remove a learning objective from a sprint. Requires Sprints.Delete permission.</summary>
public static class RemoveSprintLearningObjectiveEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:int}/learning-objectives/{loId:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Sprints.Delete);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        int loId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveSprintLearningObjectiveCommand(id, loId), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
