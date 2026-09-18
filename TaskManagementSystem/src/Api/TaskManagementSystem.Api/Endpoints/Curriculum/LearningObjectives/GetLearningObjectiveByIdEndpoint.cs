using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.GetLearningObjectiveById;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.LearningObjectives;

/// <summary>
/// Gets a learning objective by identifier.
/// </summary>
public static class GetLearningObjectiveByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder learningObjectives)
    {
        learningObjectives.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return learningObjectives;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLearningObjectiveByIdQuery(id), cancellationToken);
        return result.ToHttpResult(objective => Results.Ok(CurriculumMapping.MapLearningObjectiveDetail(objective)));
    }
}
