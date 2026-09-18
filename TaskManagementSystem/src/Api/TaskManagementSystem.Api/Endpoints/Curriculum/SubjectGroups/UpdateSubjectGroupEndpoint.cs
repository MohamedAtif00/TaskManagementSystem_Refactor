using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.UpdateSubjectGroup;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.SubjectGroups;

/// <summary>
/// Updates an existing subject group.
/// </summary>
public static class UpdateSubjectGroupEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjectGroups)
    {
        subjectGroups.MapPut("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Update);
        return subjectGroups;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateSubjectGroupRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSubjectGroupCommand(id, request.Name), cancellationToken);
        return result.ToHttpResult(group => Results.Ok(CurriculumMapping.MapSubjectGroupDetail(group)));
    }
}
