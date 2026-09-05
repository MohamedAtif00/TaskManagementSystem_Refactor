using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Contracts.Legacy;

public static class AuthRequests
{
    public sealed class LoginDto
    {
        public string Code { get; set; } = string.Empty;
    }
}

public static class AuthResponses
{
    public sealed class AuthInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Member;
        public int Id { get; set; }
        public string? Group { get; set; } = string.Empty;
        public int Notifications { get; set; }
    }
}
