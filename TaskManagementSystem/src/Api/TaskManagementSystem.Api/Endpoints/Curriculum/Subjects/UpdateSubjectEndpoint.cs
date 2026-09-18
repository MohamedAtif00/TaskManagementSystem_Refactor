using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Subjects.UpdateSubject;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;

/// <summary>
/// Updates an existing subject.
/// </summary>
public static class UpdateSubjectEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjects)
    {
        subjects.MapPut("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Update);
        return subjects;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateSubjectRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSubjectCommand(id, request.Name, request.Description), cancellationToken);
        return result.ToHttpResult(subject => Results.Ok(CurriculumMapping.MapSubjectDetail(subject)));
    }
}
