using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Security;

public static class PermissionPolicyNames
{
    public const string PermissionsManage = $"Perm:{IdentityPermissionCodes.PermissionsManage}";
    public const string RolesManage = $"Perm:{IdentityPermissionCodes.RolesManage}";
    public const string UsersAssignRole = $"Perm:{IdentityPermissionCodes.UsersAssignRole}";
    public const string UsersView = $"Perm:{IdentityPermissionCodes.UsersView}";
    public const string UsersManage = $"Perm:{IdentityPermissionCodes.UsersManage}";
}
