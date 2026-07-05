namespace AutomatedTaskSystem.Models.Configs
{
    /// <summary>
    /// Leave-related configuration from appsettings (FromNextBalance, emergency blackout, annual reset).
    /// Dates can be "yyyy-MM-dd" (full date for specific year) or "MM-dd" (month-day; current year is used).
    /// </summary>
    public class LeaveSettings
    {
        public const string SectionName = "LeaveSettings";

        /// <summary>Maximum days a user can take from next balance (e.g. 3).</summary>
        public int FromNextBalanceMaxDays { get; set; } = 3;

        /// <summary>Start of window when FromNextBalance is allowed (e.g. "01-01" or "2025-01-01").</summary>
        public string? FromNextBalanceStartDate { get; set; }

        /// <summary>End of window when FromNextBalance is allowed (e.g. "04-30" or "2025-04-30").</summary>
        public string? FromNextBalanceEndDate { get; set; }

        /// <summary>After this date (in the year), emergency leave requests are blocked until reset (e.g. "04-30" or "2025-04-30").</summary>
        public string? EmergencyBlackoutCutoffDate { get; set; }

        /// <summary>Annual reset date when all leave balances are reset (e.g. "05-01" or "2025-05-01").</summary>
        public string? ResetDate { get; set; }
        public string? RemoveOldAnnualLeavesDate { get; set; }

        /// <summary>Parses a config date string (yyyy-MM-dd or MM-dd) for the given year. Returns null if invalid or empty.</summary>
        public static DateTime? ParseDateForYear(string? value, int year)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim();

            // MM-dd first so values like "04-30" use the requested year, not TryParse's default year.
            if (value.Length <= 5 && value.Contains('-'))
            {
                var parts = value.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out var m) && int.TryParse(parts[1], out var d))
                {
                    if (m >= 1 && m <= 12 && d >= 1 && d <= DateTime.DaysInMonth(year, m))
                        return new DateTime(year, m, d);
                }
            }

            if (DateTime.TryParse(value, out var full))
                return full;

            return null;
        }
    }
}
