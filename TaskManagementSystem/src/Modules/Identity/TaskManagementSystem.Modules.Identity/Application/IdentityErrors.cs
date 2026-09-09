using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Application;

public static class IdentityErrors
{
    public static ResultError InvalidLoginCode =>
        new("invalid_login_code", "Invalid code");

    public static ResultError UserArchived =>
        new("user_archived", "User is archived.");

    public static ResultError UserNotFound =>
        new("user_not_found", "Invalid token");

    public static ResultError InvalidRefreshToken(string message) =>
        new("invalid_refresh_token", message);

    public static ResultError PermissionNotFound =>
        new("permission_not_found", "Permission was not found.");

    public static ResultError RoleNotFound =>
        new("role_not_found", "Role was not found.");

    public static ResultError DuplicatePermissionCode =>
        new("duplicate_permission_code", "A permission with this code already exists.");

    public static ResultError DuplicateRoleName =>
        new("duplicate_role_name", "A role with this name already exists.");

    public static ResultError CannotDeleteSystemPermission =>
        new("cannot_delete_system_permission", "System permissions cannot be deleted.");

    public static ResultError CannotDeleteSystemRole =>
        new("cannot_delete_system_role", "System roles cannot be deleted.");

    public static ResultError RoleInUse =>
        new("role_in_use", "Role is assigned to one or more users.");

    public static ResultError InvalidPermissionCode(string message) =>
        new("invalid_permission_code", message);

    public static ResultError InvalidPermissionName(string message) =>
        new("invalid_permission_name", message);

    public static ResultError InvalidRoleName(string message) =>
        new("invalid_role_name", message);
}
