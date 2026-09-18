using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsBySubjectGroup;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.SubjectGroups;

/// <summary>
/// Lists subjects for a subject group.
/// </summary>
public static class ListSubjectsBySubjectGroupEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjectGroups)
    {
        subjectGroups.MapGet("/{subjectGroupId:int}/subjects", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return subjectGroups;
    }

    private static async Task<IResult> HandleAsync(
        int subjectGroupId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSubjectsBySubjectGroupQuery(subjectGroupId), cancellationToken);
        return result.ToHttpResult(subjects => Results.Ok(subjects.Select(CurriculumMapping.MapSubjectListItem).ToList()));
    }
}
