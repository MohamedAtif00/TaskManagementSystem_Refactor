namespace TaskManagementSystem.Api.Contracts.Identity;

public sealed class CreatePermissionRequest
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class UpdatePermissionRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class PermissionResponse
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }
}

public sealed class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int[] PermissionIds { get; set; } = [];
}

public sealed class UpdateRoleRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class SetRolePermissionsRequest
{
    public int[] PermissionIds { get; set; } = [];
}

public sealed class RoleResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    public string[] PermissionCodes { get; set; } = [];
}

public sealed class AssignUserRoleRequest
{
    public int RoleId { get; set; }
}

public sealed class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;

    public string HrCode { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Title { get; set; }

    public int RoleId { get; set; }

    public int AccountType { get; set; }

    public int? TeamId { get; set; }
}

public sealed class UpdateUserRequest
{
    public string Name { get; set; } = string.Empty;

    public string HrCode { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Title { get; set; }

    public int RoleId { get; set; }

    public int AccountType { get; set; }

    public int? TeamId { get; set; }
}

public sealed class UserListItemResponse
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public int? TeamId { get; set; }

    public string? TeamName { get; set; }
}

public sealed class UserDetailResponse
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string HrCode { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Title { get; set; }

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public int AccountType { get; set; }

    public bool OnBoard { get; set; }

    public int? TeamId { get; set; }

    public string? TeamName { get; set; }

    public int? TeamleaderId { get; set; }

    public string? TeamleaderName { get; set; }
}

public sealed class TeamLeaderResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;
}
