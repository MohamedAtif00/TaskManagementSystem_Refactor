using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Domain;

public sealed class Role : Entity, IAggregateRoot
{
    private Role()
    {
    }

    internal static Role CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string? Description { get; internal set; }
    public bool IsSystem { get; internal set; }

    public ICollection<Permission> Permissions { get; internal set; } = [];
    public ICollection<User> Users { get; internal set; } = [];

    public static Result<Role> Create(string name, string? description, bool isSystem = false)
    {
        var normalizedName = name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Result.Fail<Role>(new ResultError("invalid_role_name", "Role name is required."));
        }

        return Result.Ok(new Role
        {
            Name = normalizedName,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsSystem = isSystem
        });
    }

    public Result<NoValue> Update(string name, string? description)
    {
        if (IsSystem)
        {
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            return Result.Ok();
        }

        var normalizedName = name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Result.Fail<NoValue>(new ResultError("invalid_role_name", "Role name is required."));
        }

        Name = normalizedName;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        return Result.Ok();
    }

    public Result<NoValue> CanDelete(bool hasAssignedUsers)
    {
        if (IsSystem)
        {
            return Result.Fail<NoValue>(new ResultError("cannot_delete_system_role", "System roles cannot be deleted."));
        }

        if (hasAssignedUsers)
        {
            return Result.Fail<NoValue>(new ResultError("role_in_use", "Role is assigned to one or more users."));
        }

        return Result.Ok();
    }

    public void ReplacePermissions(IReadOnlyCollection<Permission> permissions)
    {
        Permissions.Clear();
        foreach (var permission in permissions)
        {
            Permissions.Add(permission);
        }
    }

    public void AddPermission(Permission permission) => Permissions.Add(permission);

    public void RemovePermission(Permission permission) => Permissions.Remove(permission);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}
