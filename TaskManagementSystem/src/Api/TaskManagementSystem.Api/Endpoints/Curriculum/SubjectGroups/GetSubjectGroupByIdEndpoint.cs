using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.GetSubjectGroupById;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.SubjectGroups;

/// <summary>
/// Gets a subject group by identifier.
/// </summary>
public static class GetSubjectGroupByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjectGroups)
    {
        subjectGroups.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return subjectGroups;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSubjectGroupByIdQuery(id), cancellationToken);
        return result.ToHttpResult(group => Results.Ok(CurriculumMapping.MapSubjectGroupDetail(group)));
    }
}
