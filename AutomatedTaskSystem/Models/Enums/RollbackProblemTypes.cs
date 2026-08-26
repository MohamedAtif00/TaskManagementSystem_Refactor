namespace AutomatedTaskSystem.Models.Enums;

public static class RollbackProblemTypes
{
    public static readonly string[] All =
    [
        "Content",
        "Logic",
        "UI",
        "API",
        "Performance",
        "Development",
        "VO/Narration",
        "Others"
    ];

    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = "";
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var match = All.FirstOrDefault(t => t.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match is null)
            return false;

        normalized = match;
        return true;
    }

    public static bool TryNormalizeMany(IEnumerable<string>? values, out string joined)
    {
        joined = "";
        var normalized = new List<string>();

        foreach (var value in values ?? [])
        {
            foreach (var part in value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (TryNormalize(part, out var one) &&
                    !normalized.Contains(one, StringComparer.OrdinalIgnoreCase))
                    normalized.Add(one);
            }
        }

        if (normalized.Count == 0)
            return false;

        joined = string.Join(", ", normalized);
        return true;
    }
}
