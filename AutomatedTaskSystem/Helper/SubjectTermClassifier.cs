using System.Text.RegularExpressions;

namespace AutomatedTaskSystem.Helper;

/// <summary>
/// Parses subject codes like <c>ara_1k_1a</c>, <c>ara_3r_2e</c> — the last segment is
/// <c>{termNumber}{a|e}</c> where the digit is the term (1 → Term 1, 2 → Term 2).
/// </summary>
public static partial class SubjectTermClassifier
{
    public const int MinTermNumber = 1;
    public const int MaxTermNumber = 2;

    /// <summary>Returns term number (1 or 2), or null if the name does not match the pattern.</summary>
    public static int? TryParseTermNumberFromSubjectName(string? subjectName)
    {
        if (string.IsNullOrWhiteSpace(subjectName))
            return null;

        var lastSegment = subjectName.Trim().Split('_', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        if (string.IsNullOrEmpty(lastSegment))
            return null;

        var match = TermSuffixRegex().Match(lastSegment);
        if (!match.Success)
            return null;

        if (!int.TryParse(match.Groups["term"].Value, out var termNumber) || termNumber < 1)
            return null;

        return termNumber;
    }

    /// <summary>Maps parsed term number to Term 1 / Term 2; unknown names default to Term 1.</summary>
    public static int ResolveTermNumberFromSubjectName(string? subjectName)
    {
        var parsed = TryParseTermNumberFromSubjectName(subjectName);
        if (parsed is null)
            return MinTermNumber;

        return Math.Clamp(parsed.Value, MinTermNumber, MaxTermNumber);
    }

    /// <summary>Reads term number from a term row name such as "Term 1" or legacy "Term2".</summary>
    public static int ResolveTermNumberFromTermName(string? termName, int termOrder = 0)
    {
        if (!string.IsNullOrWhiteSpace(termName))
        {
            var match = TermNameDigitRegex().Match(termName.Trim());
            if (match.Success && int.TryParse(match.Groups["term"].Value, out var n) && n >= 1)
                return Math.Clamp(n, MinTermNumber, MaxTermNumber);
        }

        return Math.Clamp(termOrder + 1, MinTermNumber, MaxTermNumber);
    }

    public static string TermDisplayName(int termNumber) =>
        termNumber switch
        {
            1 => "Term 1",
            2 => "Term 2",
            _ => $"Term {termNumber}",
        };

    [GeneratedRegex(@"^(?<term>\d+)[ae]$", RegexOptions.IgnoreCase)]
    private static partial Regex TermSuffixRegex();

    [GeneratedRegex(@"(?<term>\d+)", RegexOptions.None)]
    private static partial Regex TermNameDigitRegex();
}
