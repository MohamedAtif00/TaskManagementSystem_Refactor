namespace TaskManagementSystem.BuildingBlocks.Domain;

public static class LoCodeFormatter
{
    private const string Separator = " · ";

    public static string Format(string? raw, string language)
    {
        var lang = string.Equals(language, "ar", StringComparison.OrdinalIgnoreCase)
            ? LoCodeLanguage.Ar
            : LoCodeLanguage.En;
        return Format(raw, lang);
    }

    public static string Format(string? raw, LoCodeLanguage language = LoCodeLanguage.En)
    {
        if (!LoCodeParser.TryParse(raw, out var code) || code is null)
        {
            return raw ?? string.Empty;
        }

        return Format(code, language);
    }

    public static string Format(LoCode code, LoCodeLanguage language = LoCodeLanguage.En)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(code.Prefix))
        {
            parts.Add(code.Prefix);
        }

        if (code.Year is int year)
        {
            parts.Add(year.ToString());
        }

        parts.Add(LoCodeCatalog.SubjectName(code.SubjectCode, language));
        parts.Add(LoCodeCatalog.GradeLabel(code.Grade, language));
        parts.Add(LoCodeCatalog.TermLabel(code.TermNumber, code.Track, language));
        parts.Add(LoCodeCatalog.UnitLabel(code.Unit, language));
        parts.Add(LoCodeCatalog.LessonLabel(code.Lesson, language));
        parts.Add(LoCodeCatalog.LoIndexLabel(code.LoIndex, language));

        var suffix = LoCodeCatalog.SuffixLabel(code.Suffix, language);
        if (!string.IsNullOrWhiteSpace(suffix))
        {
            parts.Add(suffix);
        }

        return string.Join(Separator, parts);
    }
}
