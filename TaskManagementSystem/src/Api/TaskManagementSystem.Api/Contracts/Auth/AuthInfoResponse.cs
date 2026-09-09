namespace TaskManagementSystem.Api.Contracts.Auth;

public sealed class AuthInfoResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Role { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string[] Permissions { get; set; } = [];

    public string? Group { get; set; }

    public int Notifications { get; set; }
}
