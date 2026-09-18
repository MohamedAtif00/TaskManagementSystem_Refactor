namespace TaskManagementSystem.Api.Security;

public static class PermissionPolicyNames
{
    public const string PolicyPrefix = "Perm:";

    public static string For(string permissionCode) => $"{PolicyPrefix}{permissionCode}";
}
