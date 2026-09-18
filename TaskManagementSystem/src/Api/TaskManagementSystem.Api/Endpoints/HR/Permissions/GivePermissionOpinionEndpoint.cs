using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Permissions.GivePermissionOpinion;

namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

public static class GivePermissionOpinionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder permissions)
    {
        permissions.MapPost("/{id:int}/opinions", HandleAsync).RequirePermissionCode(PermissionCodes.HrTimeoff.Update);
        return permissions;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        GivePermissionOpinionRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(
            new GivePermissionOpinionCommand(userId, role, id, request.IsApproved, request.Comment),
            cancellationToken);

        return result.ToHttpResult(permission => Results.Ok(PermissionMapping.MapPermission(permission)));
    }
}
