using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
 using TaskManagementSystem.Modules.Curriculum.Features.Subjects.ArchiveSubject;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;

/// <summary>
/// Archives a subject.
/// </summary>
public static class ArchiveSubjectEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjects)
    {
        subjects.MapDelete("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Delete);
        return subjects;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveSubjectCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
