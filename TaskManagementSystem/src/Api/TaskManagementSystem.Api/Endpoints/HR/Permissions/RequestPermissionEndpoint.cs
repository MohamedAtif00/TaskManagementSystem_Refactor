using MediatR;
using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.HR.Features.Permissions.RequestPermission;

namespace TaskManagementSystem.Api.Endpoints.HR.Permissions;

public static class RequestPermissionEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder permissions)
    {
        permissions.MapPost("", HandleAsync).RequireAuthorization();
        return permissions;
    }

    private static async Task<IResult> HandleAsync(
        CreatePermissionRequest request,
        IMediator mediator,
        ICurrentUserAccessor currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var role = currentUser.GetRequiredRole();
        var type = PermissionMapping.ParsePermissionType(request.Type);
        if (type is null || !TimeOnly.TryParse(request.FromTime, out var fromTime) || !TimeOnly.TryParse(request.ToTime, out var toTime))
        {
            return Results.BadRequest();
        }

        var result = await mediator.Send(
            new RequestPermissionCommand(userId, role, type.Value, request.PermissionDate, fromTime, toTime, request.Reason),
            cancellationToken);

        return result.ToHttpResult(permission =>
            Results.Created($"/hr/permissions/{permission.Id}", PermissionMapping.MapPermission(permission)));
    }
}
