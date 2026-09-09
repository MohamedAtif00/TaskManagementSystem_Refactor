using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Application;

public sealed record PermissionDto(
    int Id,
    string Code,
    string Name,
    string? Description,
    bool IsSystem)
{
    public static PermissionDto From(Permission permission) =>
        new(permission.Id, permission.Code, permission.Name, permission.Description, permission.IsSystem);
}

public sealed record RoleDto(
    int Id,
    string Name,
    string? Description,
    bool IsSystem,
    IReadOnlyList<string> PermissionCodes)
{
    public static RoleDto From(Role role) =>
        new(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystem,
            role.Permissions.Select(permission => permission.Code).OrderBy(code => code).ToList());
}
