using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Features.Permissions;
using TaskManagementSystem.Modules.Identity.Features.Roles;
using TaskManagementSystem.Modules.Identity.Features.Users;

namespace TaskManagementSystem.Api.Endpoints;

public static class IdentityEndpoints
{
    public static RouteGroupBuilder MapIdentityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/identity").WithTags("Identity").RequireAuthorization();

        group.MapGet("/permissions", ListPermissionsAsync)
            .RequirePermission(PermissionPolicyNames.PermissionsManage);
        group.MapPost("/permissions", CreatePermissionAsync)
            .RequirePermission(PermissionPolicyNames.PermissionsManage);
        group.MapPut("/permissions/{id:int}", UpdatePermissionAsync)
            .RequirePermission(PermissionPolicyNames.PermissionsManage);
        group.MapDelete("/permissions/{id:int}", DeletePermissionAsync)
            .RequirePermission(PermissionPolicyNames.PermissionsManage);

        group.MapGet("/roles", ListRolesAsync)
            .RequirePermission(PermissionPolicyNames.RolesManage);
        group.MapPost("/roles", CreateRoleAsync)
            .RequirePermission(PermissionPolicyNames.RolesManage);
        group.MapPut("/roles/{id:int}", UpdateRoleAsync)
            .RequirePermission(PermissionPolicyNames.RolesManage);
        group.MapDelete("/roles/{id:int}", DeleteRoleAsync)
            .RequirePermission(PermissionPolicyNames.RolesManage);
        group.MapPut("/roles/{id:int}/permissions", SetRolePermissionsAsync)
            .RequirePermission(PermissionPolicyNames.RolesManage);
        group.MapPost("/roles/{id:int}/permissions/{permissionId:int}", AddRolePermissionAsync)
            .RequirePermission(PermissionPolicyNames.RolesManage);
        group.MapDelete("/roles/{id:int}/permissions/{permissionId:int}", RemoveRolePermissionAsync)
            .RequirePermission(PermissionPolicyNames.RolesManage);

        group.MapPut("/users/{userId:int}/role", AssignUserRoleAsync)
            .RequirePermission(PermissionPolicyNames.UsersAssignRole);

        return group;
    }

    private static async Task<IResult> ListPermissionsAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListPermissionsQuery(), cancellationToken);
        return result.ToHttpResult(permissions => Results.Ok(permissions.Select(ToPermissionResponse)));
    }

    private static async Task<IResult> CreatePermissionAsync(
        CreatePermissionRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreatePermissionCommand(request.Code, request.Name, request.Description),
            cancellationToken);

        return result.ToHttpResult(permission => Results.Created($"/identity/permissions/{permission.Id}", ToPermissionResponse(permission)));
    }

    private static async Task<IResult> UpdatePermissionAsync(
        int id,
        UpdatePermissionRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdatePermissionCommand(id, request.Name, request.Description),
            cancellationToken);

        return result.ToHttpResult(permission => Results.Ok(ToPermissionResponse(permission)));
    }

    private static async Task<IResult> DeletePermissionAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeletePermissionCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> ListRolesAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListRolesQuery(), cancellationToken);
        return result.ToHttpResult(roles => Results.Ok(roles.Select(ToRoleResponse)));
    }

    private static async Task<IResult> CreateRoleAsync(
        CreateRoleRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateRoleCommand(request.Name, request.Description, request.PermissionIds),
            cancellationToken);

        return result.ToHttpResult(role => Results.Created($"/identity/roles/{role.Id}", ToRoleResponse(role)));
    }

    private static async Task<IResult> UpdateRoleAsync(
        int id,
        UpdateRoleRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateRoleCommand(id, request.Name, request.Description),
            cancellationToken);

        return result.ToHttpResult(role => Results.Ok(ToRoleResponse(role)));
    }

    private static async Task<IResult> DeleteRoleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteRoleCommand(id), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static async Task<IResult> SetRolePermissionsAsync(
        int id,
        SetRolePermissionsRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SetRolePermissionsCommand(id, request.PermissionIds),
            cancellationToken);

        return result.ToHttpResult(role => Results.Ok(ToRoleResponse(role)));
    }

    private static async Task<IResult> AddRolePermissionAsync(
        int id,
        int permissionId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AddRolePermissionCommand(id, permissionId), cancellationToken);
        return result.ToHttpResult(role => Results.Ok(ToRoleResponse(role)));
    }

    private static async Task<IResult> RemoveRolePermissionAsync(
        int id,
        int permissionId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveRolePermissionCommand(id, permissionId), cancellationToken);
        return result.ToHttpResult(role => Results.Ok(ToRoleResponse(role)));
    }

    private static async Task<IResult> AssignUserRoleAsync(
        int userId,
        AssignUserRoleRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AssignUserRoleCommand(userId, request.RoleId), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }

    private static PermissionResponse ToPermissionResponse(PermissionDto permission) =>
        new()
        {
            Id = permission.Id,
            Code = permission.Code,
            Name = permission.Name,
            Description = permission.Description,
            IsSystem = permission.IsSystem
        };

    private static RoleResponse ToRoleResponse(RoleDto role) =>
        new()
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            PermissionCodes = role.PermissionCodes.ToArray()
        };
}
