using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Sprints;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.AddSprintLearningObjectives;

namespace TaskManagementSystem.Api.Endpoints.Sprints.SprintLearningObjectives;

/// <summary>POST /sprints/{id}/learning-objectives — add learning objectives to a sprint. Requires Sprints.Create permission.</summary>
public static class AddSprintLearningObjectivesEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}/learning-objectives", HandleAsync).RequirePermissionCode(PermissionCodes.Sprints.Create);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        AddSprintLearningObjectivesRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AddSprintLearningObjectivesCommand(id, request.LearningObjectiveIds),
            cancellationToken);

        return result.ToHttpResult(_ => Results.NoContent());
    }
}
