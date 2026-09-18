using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Endpoints.Curriculum;
 using TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectUsers;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;

/// <summary>
/// Lists users assigned to a subject.
/// </summary>
public static class ListSubjectUsersEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjects)
    {
        subjects.MapGet("/{id:int}/users", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Read);
        return subjects;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSubjectUsersQuery(id), cancellationToken);
        return result.ToHttpResult(users => Results.Ok(users.Select(CurriculumMapping.MapSubjectUser).ToList()));
    }
}
