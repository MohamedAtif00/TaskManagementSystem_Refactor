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
