using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Application;

public static class IdentityResultMapper
{
    public static ResultError ToApplicationError(ResultError domainError) =>
        domainError.Code switch
        {
            "user_archived" => IdentityErrors.UserArchived,
            "invalid_refresh_token" => IdentityErrors.InvalidRefreshToken(domainError.Message),
            "invalid_login_code" => IdentityErrors.InvalidLoginCode,
            "permission_not_found" => IdentityErrors.PermissionNotFound,
            "role_not_found" => IdentityErrors.RoleNotFound,
            "duplicate_permission_code" => IdentityErrors.DuplicatePermissionCode,
            "duplicate_role_name" => IdentityErrors.DuplicateRoleName,
            "cannot_delete_system_permission" => IdentityErrors.CannotDeleteSystemPermission,
            "cannot_delete_system_role" => IdentityErrors.CannotDeleteSystemRole,
            "role_in_use" => IdentityErrors.RoleInUse,
            "invalid_permission_code" => IdentityErrors.InvalidPermissionCode(domainError.Message),
            "invalid_permission_name" => IdentityErrors.InvalidPermissionName(domainError.Message),
            "invalid_role_name" => IdentityErrors.InvalidRoleName(domainError.Message),
            _ => domainError
        };
}
