using MediatR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Permissions.GetPermissionRequestById;

namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

public static class GetPermissionRequestByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder permissions)
    {
        permissions.MapGet("/{id:int}", HandleAsync).RequireAuthorization();
        return permissions;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(new GetPermissionRequestByIdQuery(userId, role, id), cancellationToken);

        return result.ToHttpResult(permission => Results.Ok(PermissionMapping.MapPermission(permission)));
    }
}
