using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Subjects.UpdateSubjectStatus;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;

/// <summary>
/// Updates the status of a subject.
/// </summary>
public static class UpdateSubjectStatusEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjects)
    {
        subjects.MapPut("/{id:int}/status", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Update);
        return subjects;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateSubjectStatusRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSubjectStatusCommand(id, request.Status), cancellationToken);
        return result.ToHttpResult(subject => Results.Ok(CurriculumMapping.MapSubjectDetail(subject)));
    }
}
