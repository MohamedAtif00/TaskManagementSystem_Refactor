using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Features.Permissions;
using TaskManagementSystem.Modules.Identity.Features.Roles;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Features.Users;
using TaskManagementSystem.Modules.Identity.Features.Users.ArchiveUser;
using TaskManagementSystem.Modules.Identity.Features.Users.CreateUser;
using TaskManagementSystem.Modules.Identity.Features.Users.GetTeamLeaders;
using TaskManagementSystem.Modules.Identity.Features.Users.GetUserById;
using TaskManagementSystem.Modules.Identity.Features.Users.ListUsers;
using TaskManagementSystem.Modules.Identity.Features.Users.UpdateUser;

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

        group.MapGet("/users", ListUsersAsync)
            .RequirePermission(PermissionPolicyNames.UsersView);
        group.MapGet("/users/team-leaders", GetTeamLeadersAsync)
            .RequirePermission(PermissionPolicyNames.UsersView);
        group.MapGet("/users/{userId:int}", GetUserByIdAsync)
            .RequirePermission(PermissionPolicyNames.UsersView);
        group.MapPost("/users", CreateUserAsync)
            .RequirePermission(PermissionPolicyNames.UsersManage);
        group.MapPut("/users/{userId:int}", UpdateUserAsync)
            .RequirePermission(PermissionPolicyNames.UsersManage);
        group.MapDelete("/users/{userId:int}", ArchiveUserAsync)
            .RequirePermission(PermissionPolicyNames.UsersManage);

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

    private static async Task<IResult> ListUsersAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListUsersQuery(), cancellationToken);
        return result.ToHttpResult(users => Results.Ok(users.Select(ToUserListItemResponse)));
    }

    private static async Task<IResult> GetTeamLeadersAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTeamLeadersQuery(), cancellationToken);
        return result.ToHttpResult(leaders => Results.Ok(leaders.Select(ToTeamLeaderResponse)));
    }

    private static async Task<IResult> GetUserByIdAsync(
        int userId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery(userId), cancellationToken);
        return result.ToHttpResult(user => Results.Ok(ToUserDetailResponse(user)));
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateUserCommand(
                request.Name,
                request.HrCode,
                request.Email,
                request.Phone,
                request.Title,
                request.RoleId,
                (AccountType)request.AccountType,
                request.TeamId,
                request.TeamleaderId),
            cancellationToken);

        return result.ToHttpResult(user => Results.Created($"/identity/users/{user.Id}", ToUserDetailResponse(user)));
    }

    private static async Task<IResult> UpdateUserAsync(
        int userId,
        UpdateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateUserCommand(
                userId,
                request.Name,
                request.HrCode,
                request.Email,
                request.Phone,
                request.Title,
                request.RoleId,
                (AccountType)request.AccountType,
                request.TeamId,
                request.TeamleaderId),
            cancellationToken);

        return result.ToHttpResult(user => Results.Ok(ToUserDetailResponse(user)));
    }

    private static async Task<IResult> ArchiveUserAsync(
        int userId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveUserCommand(userId), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
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

    private static UserListItemResponse ToUserListItemResponse(UserListItemResult user) =>
        new()
        {
            Id = user.Id,
            Code = user.Code,
            Name = user.Name,
            RoleId = user.RoleId,
            RoleName = user.RoleName,
            TeamId = user.TeamId,
            TeamName = user.TeamName
        };

    private static UserDetailResponse ToUserDetailResponse(UserDetailResult user) =>
        new()
        {
            Id = user.Id,
            Code = user.Code,
            Name = user.Name,
            HrCode = user.HrCode,
            Email = user.Email,
            Phone = user.Phone,
            Title = user.Title,
            RoleId = user.RoleId,
            RoleName = user.RoleName,
            AccountType = user.AccountType,
            OnBoard = user.OnBoard,
            TeamId = user.TeamId,
            TeamName = user.TeamName,
            TeamleaderId = user.TeamleaderId,
            TeamleaderName = user.TeamleaderName
        };

    private static TeamLeaderResponse ToTeamLeaderResponse(TeamLeaderResult leader) =>
        new()
        {
            Id = leader.Id,
            Name = leader.Name,
            RoleId = leader.RoleId,
            RoleName = leader.RoleName
        };
}
