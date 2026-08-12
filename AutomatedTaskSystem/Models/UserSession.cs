using System.ComponentModel.DataAnnotations;
using AutomatedTaskSystem.Models.Enums;

namespace AutomatedTaskSystem.Models;

public class UserSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;
    public DateTime? LogoutAt { get; set; }
    public SessionLogoutReason? LogoutReason { get; set; }

    /// <summary>Refresh token that opened this session (RefreshToken.Token PK).</summary>
    [MaxLength(512)]
    public string RefreshToken { get; set; } = "";

    [MaxLength(64)]
    public string? IpAddress { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }
}
