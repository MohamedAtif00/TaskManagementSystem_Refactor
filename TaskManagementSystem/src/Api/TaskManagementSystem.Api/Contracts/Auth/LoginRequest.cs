namespace TaskManagementSystem.Api.Contracts.Auth;

public sealed class LoginRequest
{
    public string Code { get; set; } = string.Empty;
}
