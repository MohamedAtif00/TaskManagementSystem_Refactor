using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public static class ForgotClockPunchTypeMapping
{
    public static ForgotClockPunchType? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.ToLowerInvariant() switch
        {
            "in" => ForgotClockPunchType.ClockIn,
            "out" => ForgotClockPunchType.ClockOut,
            _ => Enum.TryParse<ForgotClockPunchType>(value, true, out var punchType)
                ? punchType
                : null
        };
    }

    public static ForgotClockPunchType ParseFromStorage(string value) =>
        Parse(value)
        ?? throw new ArgumentException($"Requested value '{value}' was not found.", nameof(value));

    public static IReadOnlyList<string> GetStorageValues(ForgotClockPunchType punchType) =>
        punchType switch
        {
            ForgotClockPunchType.ClockIn => ["ClockIn", "In"],
            ForgotClockPunchType.ClockOut => ["ClockOut", "Out"],
            _ => [punchType.ToString()]
        };
}
