using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
 using TaskManagementSystem.Modules.Curriculum.Features.Subjects.AssignSubjectUsers;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;

/// <summary>
/// Assigns users to a subject.
/// </summary>
public static class AssignSubjectUsersEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjects)
    {
        subjects.MapPost("/{id:int}/users", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Create);
        return subjects;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        AssignSubjectUsersRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AssignSubjectUsersCommand(id, request.UserIds), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
