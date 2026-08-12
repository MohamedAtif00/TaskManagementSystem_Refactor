using AutomatedTaskSystem.Models.Enums;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.SessionTracking;

public interface IUserSessionService
{
    System.Threading.Tasks.Task StartSessionAsync(int userId, string refreshToken, string? ipAddress, string? userAgent);
    System.Threading.Tasks.Task EndSessionByRefreshTokenAsync(string refreshToken, SessionLogoutReason reason);
    System.Threading.Tasks.Task EndOpenSessionsForUserAsync(int userId, SessionLogoutReason reason);
    System.Threading.Tasks.Task InvalidateRefreshTokensForUserAsync(int userId);
    System.Threading.Tasks.Task CloseExpiredSessionsAsync();
    System.Threading.Tasks.Task<ActionResult<ResponseService<UserSessionListDto>>> GetSessionsAsync(UserSessionFilterDto filter);
    System.Threading.Tasks.Task<ActionResult<ResponseService<UserDayStorylineDto>>> GetDayStorylineAsync(int userId, DateTime date);
}

public class UserSessionFilterDto
{
    public int? UserId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public SessionLogoutReason? Reason { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class UserSessionListDto
{
    public List<UserSessionRowDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class UserSessionRowDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
    public DateTime LoginAt { get; set; }
    public DateTime? LogoutAt { get; set; }
    public long? DurationMinutes { get; set; }
    public decimal? TotalHours { get; set; }
    public string DurationFormatted { get; set; } = "";
    /// <summary>Human-readable logout reason (e.g. "Logged out by system (session expired)").</summary>
    public string? Reason { get; set; }
    /// <summary>Raw enum name when closed (Manual, TokenExpired, ...).</summary>
    public string? ReasonCode { get; set; }
    public string? IpAddress { get; set; }
}

public class UserDayStorylineDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
    public DateTime Date { get; set; }
    public DateTime DayStart { get; set; }
    public DateTime DayEnd { get; set; }
    public decimal ActiveHours { get; set; }
    public decimal InactiveHours { get; set; }
    public int SessionCount { get; set; }
    public List<DayStorySegmentDto> Segments { get; set; } = new();
    public List<UserSessionRowDto> Sessions { get; set; } = new();
}

public class DayStorySegmentDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    /// <summary>Active or Inactive</summary>
    public string Kind { get; set; } = "Inactive";
    public decimal Hours { get; set; }
    public string DurationFormatted { get; set; } = "";
    public string? Reason { get; set; }
    public int? SessionId { get; set; }
}
