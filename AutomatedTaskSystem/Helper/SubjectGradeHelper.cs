using System.Text.RegularExpressions;

namespace AutomatedTaskSystem.Helper;

public static partial class SubjectGradeHelper
{
    public static string? TryParseGradeCode(string? subjectName)
    {
        if (string.IsNullOrWhiteSpace(subjectName))
            return null;

        var parts = subjectName.Trim().Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return null;

        return parts[1].ToLowerInvariant();
    }

    public static string ToDisplayGrade(string? gradeCode)
    {
        if (string.IsNullOrWhiteSpace(gradeCode))
            return "";

        return gradeCode.ToLowerInvariant() switch
        {
            "1k" => "Kg1",
            "2k" => "Kg2",
            "1r" => "Grade 1",
            "2r" => "Grade 2",
            "3r" => "Grade 3",
            "4r" => "Grade 4",
            "5r" => "Grade 5",
            "6r" => "Grade 6",
            "7r" => "Grade 7",
            _ => gradeCode
        };
    }

    public static string ResolveSubjectDiscipline(string? subjectName)
    {
        if (string.IsNullOrWhiteSpace(subjectName))
            return "Other";

        var lower = subjectName.ToLowerInvariant().Trim();
        var prefixMatch = SubjectPrefixRegex().Match(lower);
        if (!prefixMatch.Success)
            return "Other";

        return prefixMatch.Groups[1].Value switch
        {
            "ara" => "Arabic",
            "eng" => "English",
            "mth" => "Math (A)",
            "sci" => "Science (A)",
            "soc" => "Social",
            "ict" => "ICT (A)",
            "mul" => "MUL (A)",
            "rel" => "Religion",
            _ => char.ToUpper(prefixMatch.Groups[1].Value[0]) + prefixMatch.Groups[1].Value[1..]
        };
    }

    [GeneratedRegex(@"^([^_]+)_")]
    private static partial Regex SubjectPrefixRegex();
}
