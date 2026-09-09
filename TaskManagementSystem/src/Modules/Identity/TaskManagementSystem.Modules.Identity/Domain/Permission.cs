using System.Text.RegularExpressions;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Domain;

public sealed partial class Permission : Entity, IAggregateRoot
{
    private static readonly Regex CodePattern = PermissionCodeRegex();

    private Permission()
    {
    }

    internal static Permission CreateForPersistence() => new();

    public int Id { get; internal set; }
    public string Code { get; internal set; } = string.Empty;
    public string Name { get; internal set; } = string.Empty;
    public string? Description { get; internal set; }
    public bool IsSystem { get; internal set; }

    public ICollection<Role> Roles { get; internal set; } = [];

    public static Result<Permission> Create(string code, string name, string? description, bool isSystem = false)
    {
        var normalizedCode = code.Trim().ToLowerInvariant();
        var normalizedName = name.Trim();

        if (!CodePattern.IsMatch(normalizedCode))
        {
            return Result.Fail<Permission>(new ResultError(
                "invalid_permission_code",
                "Permission code must use lowercase dotted segments (e.g. hr.holidays.manage)."));
        }

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Result.Fail<Permission>(new ResultError("invalid_permission_name", "Permission name is required."));
        }

        return Result.Ok(new Permission
        {
            Code = normalizedCode,
            Name = normalizedName,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsSystem = isSystem
        });
    }

    public Result<NoValue> Update(string name, string? description)
    {
        var normalizedName = name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Result.Fail<NoValue>(new ResultError("invalid_permission_name", "Permission name is required."));
        }

        Name = normalizedName;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        return Result.Ok();
    }

    public Result<NoValue> CanDelete() =>
        IsSystem
            ? Result.Fail<NoValue>(new ResultError("cannot_delete_system_permission", "System permissions cannot be deleted."))
            : Result.Ok();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }

    [GeneratedRegex(@"^[a-z][a-z0-9]*(\.[a-z][a-z0-9]*)+$", RegexOptions.CultureInvariant)]
    private static partial Regex PermissionCodeRegex();
}
