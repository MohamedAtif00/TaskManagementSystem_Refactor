namespace TaskManagementSystem.Modules.Identity.Domain;

public static class IdentityPermissionCodes
{
    public const string PermissionsManage = "identity.permissions.manage";
    public const string RolesManage = "identity.roles.manage";
    public const string UsersAssignRole = "identity.users.assign-role";
    public const string UsersView = "identity.users.view";
    public const string UsersManage = "identity.users.manage";

    public static readonly IReadOnlyCollection<string> SystemCodes =
    [
        PermissionsManage,
        RolesManage,
        UsersAssignRole,
        UsersView,
        UsersManage
    ];
}
