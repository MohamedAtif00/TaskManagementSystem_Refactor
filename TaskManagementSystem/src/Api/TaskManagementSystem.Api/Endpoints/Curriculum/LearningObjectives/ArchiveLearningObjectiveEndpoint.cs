using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
 using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.ArchiveLearningObjective;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.LearningObjectives;

/// <summary>
/// Archives a learning objective.
/// </summary>
public static class ArchiveLearningObjectiveEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder learningObjectives)
    {
        learningObjectives.MapDelete("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Delete);
        return learningObjectives;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveLearningObjectiveCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
