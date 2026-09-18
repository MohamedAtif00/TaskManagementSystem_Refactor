using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Subjects.CreateSubject;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.SubjectGroups;

/// <summary>
/// Creates a subject under a subject group.
/// </summary>
public static class CreateSubjectEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjectGroups)
    {
        subjectGroups.MapPost("/{subjectGroupId:int}/subjects", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Create);
        return subjectGroups;
    }

    private static async Task<IResult> HandleAsync(
        int subjectGroupId,
        CreateSubjectRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateSubjectCommand(subjectGroupId, request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(subject => Results.Created($"/curriculum/subjects/{subject.Id}", CurriculumMapping.MapSubjectDetail(subject)));
    }
}
