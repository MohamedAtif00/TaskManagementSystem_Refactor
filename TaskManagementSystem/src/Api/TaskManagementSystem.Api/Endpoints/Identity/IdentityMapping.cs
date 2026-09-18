using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Features.Users;

namespace TaskManagementSystem.Api.Endpoints.Identity;

internal static class IdentityMapping
{
    internal static PermissionResponse ToPermissionResponse(PermissionDto permission) =>
        new()
        {
            Id = permission.Id,
            Code = permission.Code,
            Name = permission.Name,
            Description = permission.Description,
            IsSystem = permission.IsSystem
        };

    internal static RoleResponse ToRoleResponse(RoleDto role) =>
        new()
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            PermissionCodes = role.PermissionCodes.ToArray()
        };

    internal static UserListItemResponse ToUserListItemResponse(UserListItemResult user) =>
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

    internal static UserDetailResponse ToUserDetailResponse(UserDetailResult user) =>
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

    internal static TeamLeaderResponse ToTeamLeaderResponse(TeamLeaderResult leader) =>
        new()
        {
            Id = leader.Id,
            Name = leader.Name,
            RoleId = leader.RoleId,
            RoleName = leader.RoleName
        };
}
