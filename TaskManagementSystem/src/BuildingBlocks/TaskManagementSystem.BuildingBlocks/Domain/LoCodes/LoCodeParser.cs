using System.Globalization;
using System.Text.RegularExpressions;

namespace TaskManagementSystem.BuildingBlocks.Domain;

public static partial class LoCodeParser
{
    public static bool TryParse(string? raw, out LoCode? code)
    {
        code = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var match = Pattern().Match(raw.Trim());
        if (!match.Success)
        {
            return false;
        }

        var prefixGroup = match.Groups["prefix"];
        var yearGroup = match.Groups["year"];
        var suffixGroup = match.Groups["suffix"];

        var prefix = prefixGroup.Success ? prefixGroup.Value.ToUpperInvariant() : null;
        int? year = yearGroup.Success ? int.Parse(yearGroup.Value, CultureInfo.InvariantCulture) : null;
        var subjectCode = match.Groups["subject"].Value.ToLowerInvariant();
        var grade = int.Parse(match.Groups["grade"].Value, CultureInfo.InvariantCulture);
        var termNumber = int.Parse(match.Groups["termNumber"].Value, CultureInfo.InvariantCulture);
        var track = char.ToUpperInvariant(match.Groups["track"].Value[0]);
        var unit = int.Parse(match.Groups["unit"].Value, CultureInfo.InvariantCulture);
        var lesson = int.Parse(match.Groups["lesson"].Value, CultureInfo.InvariantCulture);
        var loIndex = int.Parse(match.Groups["lo"].Value, CultureInfo.InvariantCulture);
        var suffix = suffixGroup.Success ? suffixGroup.Value : null;

        code = new LoCode(prefix, year, subjectCode, grade, termNumber, track, unit, lesson, loIndex, suffix);
        return true;
    }

    [GeneratedRegex(
        @"^(?:(?<prefix>QR)_)?(?:(?<year>\d{4})_)?(?<subject>[A-Za-z]{3})_(?<grade>\d{1,2})[Rr]_(?<termNumber>[12])(?<track>[AEae])_(?<unit>\d{2})_(?<lesson>\d{2})_(?<lo>\d{2})(?:_(?<suffix>.+))?$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex Pattern();
}
