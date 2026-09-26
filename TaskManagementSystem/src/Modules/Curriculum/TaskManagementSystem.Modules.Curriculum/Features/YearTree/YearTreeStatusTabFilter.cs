using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.YearTree;

public static class YearTreeStatusTabFilter
{
    public static int[]? TryParseStatusTab(string? statusTab, out bool valid)
    {
        valid = true;
        if (string.IsNullOrWhiteSpace(statusTab))
        {
            return null;
        }

        return statusTab.Trim().ToLowerInvariant() switch
        {
            "active" => [(int)SubjectStatus.Active, (int)SubjectStatus.Reopened],
            "hold" => [(int)SubjectStatus.Hold],
            "closed" => [(int)SubjectStatus.Closed],
            _ => Invalid(out valid),
        };
    }

    private static int[]? Invalid(out bool valid)
    {
        valid = false;
        return null;
    }
}
