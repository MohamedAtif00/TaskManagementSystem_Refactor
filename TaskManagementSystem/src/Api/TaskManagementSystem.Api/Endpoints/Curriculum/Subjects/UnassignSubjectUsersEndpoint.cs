using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Curriculum;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
 using TaskManagementSystem.Modules.Curriculum.Features.Subjects.UnassignSubjectUsers;

namespace TaskManagementSystem.Api.Endpoints.Curriculum.Subjects;

/// <summary>
/// Unassigns users from a subject.
/// </summary>
public static class UnassignSubjectUsersEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder subjects)
    {
        subjects.MapPost("/{id:int}/users/unassign", HandleAsync).RequirePermissionCode(PermissionCodes.Curriculum.Create);
        return subjects;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UnassignSubjectUsersRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UnassignSubjectUsersCommand(id, request.UserIds), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
