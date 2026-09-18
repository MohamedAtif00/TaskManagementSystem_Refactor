namespace TaskManagementSystem.Modules.Identity.Domain;

[Obsolete("Use PermissionCodes instead.")]
public static class IdentityPermissionCodes
{
    public const string RolesManage = PermissionCodes.IdentityRoles.Manage;
    public const string UsersAssignRole = PermissionCodes.IdentityUsers.Update;
    public const string UsersView = PermissionCodes.IdentityUsers.Read;
    public const string UsersManage = PermissionCodes.IdentityUsers.Manage;

    public static readonly IReadOnlyCollection<string> SystemCodes = PermissionCodes.All;
}
