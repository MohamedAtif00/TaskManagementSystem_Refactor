namespace TaskManagementSystem.Modules.HR.Domain;

public static class LeaveSettingsHelper
{
    public static DateTime? ParseDateForYear(string? value, int year)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();

        if (value.Length <= 5 && value.Contains('-'))
        {
            var parts = value.Split('-');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out var month) &&
                int.TryParse(parts[1], out var day) &&
                month >= 1 && month <= 12 &&
                day >= 1 && day <= DateTime.DaysInMonth(year, month))
            {
                return new DateTime(year, month, day);
            }
        }

        return DateTime.TryParse(value, out var full) ? full.Date : null;
    }

    public static bool IsEmergencyAllowed(LeaveSettingsOptions settings, DateTime today)
    {
        var cutoff = ParseDateForYear(settings.EmergencyBlackoutCutoffDate, today.Year);
        var resetDate = ParseDateForYear(settings.ResetDate, today.Year);

        return (cutoff is null && resetDate is null) ||
               (cutoff is not null && today <= cutoff.Value) ||
               (resetDate is not null && today >= resetDate.Value);
    }

    public static bool IsInFromNextWindow(LeaveSettingsOptions settings, DateTime today)
    {
        var windowStart = ParseDateForYear(settings.FromNextBalanceStartDate, today.Year);
        var windowEnd = ParseDateForYear(settings.FromNextBalanceEndDate, today.Year);

        return windowStart is not null &&
               windowEnd is not null &&
               today >= windowStart.Value &&
               today <= windowEnd.Value;
    }

    public static bool IsFromNextAvailable(LeaveSettingsOptions settings, DateTime today)
    {
        var resetDate = ParseDateForYear(settings.ResetDate, today.Year);
        return resetDate is null || today < resetDate.Value;
    }
}
