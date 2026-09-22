namespace TaskManagementSystem.BuildingBlocks.Domain;

public static class LoCodeCatalog
{
    private static readonly Dictionary<string, (string En, string Ar)> Subjects = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mth"] = ("Math", "الرياضيات"),
        ["ara"] = ("Arabic", "اللغة العربية"),
        ["sci"] = ("Science", "العلوم"),
        ["eng"] = ("English", "اللغة الإنجليزية"),
        ["soc"] = ("Social Studies", "الدراسات الاجتماعية"),
        ["mul"] = ("Multimedia", "الوسائط المتعددة"),
        ["rel"] = ("Religion", "التربية الدينية"),
        ["ict"] = ("ICT", "الحاسب"),
        ["tsk"] = ("Tokkatsu", "التوكاتسو")
    };

    public static string SubjectName(string subjectCode, LoCodeLanguage language)
    {
        if (!Subjects.TryGetValue(subjectCode, out var names))
        {
            return subjectCode;
        }

        return language == LoCodeLanguage.Ar ? names.Ar : names.En;
    }

    public static string GradeLabel(int grade, LoCodeLanguage language) =>
        language == LoCodeLanguage.Ar ? $"الصف {grade}" : $"Grade {grade}";

    public static string TermLabel(int termNumber, char track, LoCodeLanguage language)
    {
        var trackLabel = TrackLabel(track, language);
        if (language == LoCodeLanguage.Ar)
        {
            var term = termNumber == 2 ? "الفصل الدراسي الثاني" : "الفصل الدراسي الأول";
            return $"{term} ({trackLabel})";
        }

        return $"Term {termNumber} ({trackLabel})";
    }

    public static string TrackLabel(char track, LoCodeLanguage language)
    {
        var isEnglishTrack = char.ToUpperInvariant(track) == 'E';
        if (language == LoCodeLanguage.Ar)
        {
            return isEnglishTrack ? "إنجليزي" : "عربي";
        }

        return isEnglishTrack ? "English" : "Arabic";
    }

    public static string UnitLabel(int unit, LoCodeLanguage language) =>
        language == LoCodeLanguage.Ar ? $"الوحدة {unit}" : $"Unit {unit}";

    public static string LessonLabel(int lesson, LoCodeLanguage language) =>
        language == LoCodeLanguage.Ar ? $"الدرس {lesson}" : $"Lesson {lesson}";

    public static string LoIndexLabel(int loIndex, LoCodeLanguage language) =>
        language == LoCodeLanguage.Ar ? $"الهدف {loIndex}" : $"LO {loIndex}";

    public static string? SuffixLabel(string? suffix, LoCodeLanguage language)
    {
        if (string.IsNullOrWhiteSpace(suffix))
        {
            return null;
        }

        if (suffix.Length >= 2
            && (suffix[0] == 'p' || suffix[0] == 'P')
            && int.TryParse(suffix.AsSpan(1), out var part))
        {
            return language == LoCodeLanguage.Ar ? $"الجزء {part}" : $"Part {part}";
        }

        return suffix;
    }
}
