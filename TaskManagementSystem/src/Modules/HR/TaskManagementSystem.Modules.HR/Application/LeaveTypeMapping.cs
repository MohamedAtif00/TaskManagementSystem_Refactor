using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public static class LeaveTypeMapping
{
    public static LeaveType? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (string.Equals(value, "Unpaid", StringComparison.OrdinalIgnoreCase))
        {
            return LeaveType.UnpaidLeave;
        }

        return Enum.TryParse<LeaveType>(value, true, out var leaveType)
            ? leaveType
            : null;
    }

    public static LeaveType ParseFromStorage(string value) =>
        Parse(value)
        ?? throw new ArgumentException($"Requested value '{value}' was not found.", nameof(value));

    public static IReadOnlyList<string> GetStorageValues(LeaveType leaveType) =>
        leaveType switch
        {
            LeaveType.UnpaidLeave => ["UnpaidLeave", "Unpaid"],
            _ => [leaveType.ToString()]
        };
}
