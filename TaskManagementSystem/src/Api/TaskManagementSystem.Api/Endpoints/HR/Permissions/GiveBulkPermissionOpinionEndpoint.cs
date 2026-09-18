using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Permissions.GiveBulkPermissionOpinion;

namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

public static class GiveBulkPermissionOpinionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder permissions)
    {
        permissions.MapPost("/opinions/bulk", HandleAsync).RequirePermissionCode(PermissionCodes.HrTimeoff.Manage);
        return permissions;
    }

    private static async Task<IResult> HandleAsync(
        GiveBulkPermissionOpinionRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var result = await mediator.Send(
            new GiveBulkPermissionOpinionCommand(userId, role, request.PermissionIds, request.IsApproved, request.Comment),
            cancellationToken);

        return result.ToHttpResult(bulk => Results.Ok(new BulkPermissionOpinionResponse
        {
            Succeeded = bulk.Succeeded,
            Failed = bulk.Failed,
            FailedPermissionIds = bulk.FailedPermissionIds.ToList()
        }));
    }
}
