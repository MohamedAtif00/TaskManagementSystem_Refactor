using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Security;

public static class PermissionPolicyNames
{
    public const string PermissionsManage = $"Perm:{IdentityPermissionCodes.PermissionsManage}";
    public const string RolesManage = $"Perm:{IdentityPermissionCodes.RolesManage}";
    public const string UsersAssignRole = $"Perm:{IdentityPermissionCodes.UsersAssignRole}";
}
