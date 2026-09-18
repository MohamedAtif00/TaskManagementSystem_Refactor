using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
 using TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.ArchiveSubjectGroup;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.SubjectGroups;

/// <summary>
/// Archives a subject group.
/// </summary>
public static class ArchiveSubjectGroupEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjectGroups)
    {
        subjectGroups.MapDelete("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Delete);
        return subjectGroups;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveSubjectGroupCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
