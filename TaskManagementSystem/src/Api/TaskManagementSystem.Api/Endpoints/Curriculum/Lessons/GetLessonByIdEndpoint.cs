using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Lessons.GetLessonById;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Lessons;

/// <summary>
/// Gets a lesson by identifier.
/// </summary>
public static class GetLessonByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder lessons)
    {
        lessons.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return lessons;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLessonByIdQuery(id), cancellationToken);
        return result.ToHttpResult(lesson => Results.Ok(CurriculumMapping.MapLessonDetail(lesson)));
    }
}
