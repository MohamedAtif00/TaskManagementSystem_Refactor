using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums;
using AutomatedTaskSystem.Models.Enums.UserRole;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.TokenService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Services.SessionTracking;

public class UserSessionService : IUserSessionService
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;

    public UserSessionService(DataContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async System.Threading.Tasks.Task StartSessionAsync(int userId, string refreshToken, string? ipAddress, string? userAgent)
    {
        await EndOpenSessionsForUserAsync(userId, SessionLogoutReason.ReplacedByNewLogin, save: false);

        _context.UserSessions.Add(new Models.UserSession
        {
            UserId = userId,
            RefreshToken = refreshToken,
            LoginAt = DateTime.UtcNow,
            IpAddress = Truncate(ipAddress, 64),
            UserAgent = Truncate(userAgent, 512)
        });

        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task EndSessionByRefreshTokenAsync(string refreshToken, SessionLogoutReason reason)
    {
        var session = await _context.UserSessions
            .Where(s => s.RefreshToken == refreshToken && s.LogoutAt == null)
            .FirstOrDefaultAsync();

        if (session is null)
            return;

        session.LogoutAt = DateTime.UtcNow;
        session.LogoutReason = reason;
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task EndOpenSessionsForUserAsync(int userId, SessionLogoutReason reason)
        => await EndOpenSessionsForUserAsync(userId, reason, save: true);

    private async System.Threading.Tasks.Task EndOpenSessionsForUserAsync(int userId, SessionLogoutReason reason, bool save)
    {
        var openSessions = await _context.UserSessions
            .Where(s => s.UserId == userId && s.LogoutAt == null)
            .ToListAsync();

        if (openSessions.Count == 0)
            return;

        var now = DateTime.UtcNow;
        foreach (var session in openSessions)
        {
            session.LogoutAt = now;
            session.LogoutReason = reason;
        }

        if (save)
            await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task InvalidateRefreshTokensForUserAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.Used)
            .ToListAsync();

        foreach (var token in tokens)
            token.Used = true;

        if (tokens.Count > 0)
            await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task CloseExpiredSessionsAsync()
    {
        var now = DateTime.UtcNow;

        var openSessions = await _context.UserSessions
            .Where(s => s.LogoutAt == null)
            .ToListAsync();

        if (openSessions.Count == 0)
            return;

        var refreshTokens = await _context.RefreshTokens
            .Where(t => openSessions.Select(s => s.RefreshToken).Contains(t.Token))
            .ToDictionaryAsync(t => t.Token);

        var changed = false;
        foreach (var session in openSessions)
        {
            refreshTokens.TryGetValue(session.RefreshToken, out var token);
            if (!TryResolveSystemLogout(session, token, now, out var logoutAt, out var reason))
                continue;

            session.LogoutAt = logoutAt;
            session.LogoutReason = reason;
            if (token is not null && !token.Used)
                token.Used = true;
            changed = true;
        }

        if (changed)
            await _context.SaveChangesAsync();
    }

    /// <summary>
    /// A session ends by the system only when the linked refresh token is expired
    /// (or missing). Access JWT lifetime alone must not close the session — silent
    /// refresh keeps the same UserSession open while the user stays active.
    /// </summary>
    internal static bool TryResolveSystemLogout(
        Models.UserSession session,
        RefreshToken? token,
        DateTime utcNow,
        out DateTime logoutAt,
        out SessionLogoutReason reason)
    {
        reason = SessionLogoutReason.TokenExpired;
        logoutAt = utcNow;

        if (token is null)
        {
            // No refresh token row — cannot prove the session is still valid
            logoutAt = utcNow;
            return true;
        }

        var refreshExpiresAt = AsUtc(token.Expires);
        if (refreshExpiresAt > utcNow)
            return false;

        logoutAt = refreshExpiresAt;
        reason = SessionLogoutReason.TokenExpired;
        return true;
    }

    public async System.Threading.Tasks.Task<ActionResult<ResponseService<UserSessionListDto>>> GetSessionsAsync(UserSessionFilterDto filter)
    {
        var authUser = await GetAuthedOwnerAsync();
        if (authUser is null)
        {
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Owner access required" }
            );
        }

        // Ensure expired open sessions are closed before returning the list
        await CloseExpiredSessionsAsync();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 25 : Math.Min(filter.PageSize, 100);

        var query = _context.UserSessions
            .AsNoTracking()
            .Include(s => s.User)
            .AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(s => s.UserId == filter.UserId.Value);

        if (filter.From.HasValue)
            query = query.Where(s => s.LoginAt >= filter.From.Value.ToUniversalTime());

        if (filter.To.HasValue)
            query = query.Where(s => s.LoginAt <= filter.To.Value.ToUniversalTime());

        if (filter.Reason.HasValue)
            query = query.Where(s => s.LogoutReason == filter.Reason.Value);

        var totalCount = await query.CountAsync();

        var rows = await query
            .OrderByDescending(s => s.LoginAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = rows.Select(MapRow).ToList();

        return new ResponseService<UserSessionListDto>
        {
            Error = false,
            Message = "Sessions",
            Data = new UserSessionListDto
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        };
    }

    public async System.Threading.Tasks.Task<ActionResult<ResponseService<UserDayStorylineDto>>> GetDayStorylineAsync(
        int userId,
        DateTime date)
    {
        var authUser = await GetAuthedOwnerAsync();
        if (authUser is null)
        {
            return new UnauthorizedObjectResult(
                new BaseResponseService { Error = true, Message = "Owner access required" }
            );
        }

        await CloseExpiredSessionsAsync();

        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && !u.Archived)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return new NotFoundObjectResult(
                new BaseResponseService { Error = true, Message = "User not found" }
            );
        }

        // Calendar day in UTC (matches stored LoginAt/LogoutAt)
        var dayStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);
        var now = DateTime.UtcNow;
        var timelineEnd = now < dayEnd ? now : dayEnd;

        var sessions = await _context.UserSessions
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s =>
                s.UserId == userId &&
                s.LoginAt < dayEnd &&
                (s.LogoutAt == null || s.LogoutAt > dayStart))
            .OrderBy(s => s.LoginAt)
            .ToListAsync();

        var activeIntervals = new List<(DateTime Start, DateTime End, Models.UserSession Session)>();
        foreach (var session in sessions)
        {
            var start = AsUtc(session.LoginAt);
            var end = session.LogoutAt.HasValue ? AsUtc(session.LogoutAt.Value) : now;
            if (start < dayStart) start = dayStart;
            if (end > dayEnd) end = dayEnd;
            if (end > timelineEnd) end = timelineEnd;
            if (end <= start) continue;
            activeIntervals.Add((start, end, session));
        }

        // Merge overlapping active intervals for cleaner storyline
        var merged = MergeIntervals(activeIntervals);

        var segments = new List<DayStorySegmentDto>();
        var cursor = dayStart;

        foreach (var interval in merged)
        {
            if (interval.Start > cursor)
            {
                segments.Add(MakeSegment(cursor, interval.Start, "Inactive", null, null));
            }

            var session = interval.Session;
            segments.Add(MakeSegment(
                interval.Start,
                interval.End,
                "Active",
                FormatReasonLabel(session.LogoutReason, session.LogoutAt.HasValue),
                session.Id));

            cursor = interval.End;
        }

        if (cursor < timelineEnd)
            segments.Add(MakeSegment(cursor, timelineEnd, "Inactive", null, null));

        // If the day is in the future relative to now, show remaining day as inactive preview only up to now
        // (already handled by timelineEnd)

        var activeHours = segments.Where(s => s.Kind == "Active").Sum(s => s.Hours);
        var inactiveHours = segments.Where(s => s.Kind == "Inactive").Sum(s => s.Hours);

        return new ResponseService<UserDayStorylineDto>
        {
            Error = false,
            Message = "Day storyline",
            Data = new UserDayStorylineDto
            {
                UserId = user.Id,
                UserName = user.Name,
                Date = dayStart,
                DayStart = dayStart,
                DayEnd = dayEnd,
                ActiveHours = Math.Round(activeHours, 2),
                InactiveHours = Math.Round(inactiveHours, 2),
                SessionCount = sessions.Count,
                Segments = segments,
                Sessions = sessions.Select(MapRow).ToList()
            }
        };
    }

    private static List<(DateTime Start, DateTime End, Models.UserSession Session)> MergeIntervals(
        List<(DateTime Start, DateTime End, Models.UserSession Session)> intervals)
    {
        if (intervals.Count == 0)
            return intervals;

        var ordered = intervals.OrderBy(i => i.Start).ToList();
        var result = new List<(DateTime Start, DateTime End, Models.UserSession Session)> { ordered[0] };

        for (var i = 1; i < ordered.Count; i++)
        {
            var last = result[^1];
            var current = ordered[i];
            if (current.Start <= last.End)
            {
                var end = current.End > last.End ? current.End : last.End;
                // Keep the session that started first for label purposes
                result[^1] = (last.Start, end, last.Session);
            }
            else
            {
                result.Add(current);
            }
        }

        return result;
    }

    private static DayStorySegmentDto MakeSegment(
        DateTime start,
        DateTime end,
        string kind,
        string? reason,
        int? sessionId)
    {
        var span = end - start;
        if (span < TimeSpan.Zero) span = TimeSpan.Zero;
        return new DayStorySegmentDto
        {
            Start = start,
            End = end,
            Kind = kind,
            Hours = Math.Round((decimal)span.TotalHours, 2),
            DurationFormatted = FormatDuration(span),
            Reason = reason,
            SessionId = sessionId
        };
    }

    private async System.Threading.Tasks.Task<User?> GetAuthedOwnerAsync()
    {
        var tokenRes = _tokenService.GetUserIdFromToken();
        if (tokenRes.Error || !int.TryParse(tokenRes.Data, out int userId))
            return null;

        return await _context.Users
            .Where(u => u.Id == userId && !u.Archived && u.Role == UserRoleEnum.Owner)
            .FirstOrDefaultAsync();
    }

    private static UserSessionRowDto MapRow(Models.UserSession session)
    {
        long? minutes = null;
        decimal? totalHours = null;
        var formatted = "Active";

        if (session.LogoutAt.HasValue)
        {
            var span = AsUtc(session.LogoutAt.Value) - AsUtc(session.LoginAt);
            if (span < TimeSpan.Zero)
                span = TimeSpan.Zero;
            minutes = (long)span.TotalMinutes;
            totalHours = Math.Round((decimal)span.TotalHours, 2);
            formatted = FormatDuration(span);
        }

        var reasonCode = session.LogoutReason?.ToString();
        var reasonLabel = FormatReasonLabel(session.LogoutReason, session.LogoutAt.HasValue);

        return new UserSessionRowDto
        {
            Id = session.Id,
            UserId = session.UserId,
            UserName = session.User?.Name ?? "",
            LoginAt = AsUtc(session.LoginAt),
            LogoutAt = session.LogoutAt.HasValue ? AsUtc(session.LogoutAt.Value) : null,
            DurationMinutes = minutes,
            TotalHours = totalHours,
            DurationFormatted = formatted,
            Reason = reasonLabel,
            ReasonCode = reasonCode,
            IpAddress = session.IpAddress
        };
    }

    public static string FormatReasonLabel(SessionLogoutReason? reason, bool hasLogout)
    {
        if (!hasLogout && reason is null)
            return "Active";

        return reason switch
        {
            SessionLogoutReason.Manual => "Manual logout",
            SessionLogoutReason.TokenExpired => "Logged out by system (session expired)",
            SessionLogoutReason.InactivityTimeout => "Logged out by system (inactivity)",
            SessionLogoutReason.ForcedByAdmin => "Logged out by system (admin)",
            SessionLogoutReason.ReplacedByNewLogin => "Replaced by new login",
            null => "Logged out by system",
            _ => reason.ToString() ?? "Logged out by system"
        };
    }

    private static DateTime AsUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string FormatDuration(TimeSpan span)
    {
        if (span.TotalDays >= 1)
            return $"{(int)span.TotalDays}d {span.Hours}h {span.Minutes}m";
        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours}h {span.Minutes}m";
        if (span.TotalMinutes >= 1)
            return $"{(int)span.TotalMinutes}m";
        return $"{Math.Max(0, (int)span.TotalSeconds)}s";
    }

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        return value.Length <= max ? value : value[..max];
    }
}
