using System.Globalization;

namespace TaskManagementSystem.Api.Infrastructure;

internal static class OptionalQueryBinding
{
    public static DateTime? ParseOptionalDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : null;
    }
}
