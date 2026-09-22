namespace TaskManagementSystem.BuildingBlocks.Domain;

public sealed class LoCode : ValueObject
{
    internal LoCode(
        string? prefix,
        int? year,
        string subjectCode,
        int grade,
        int termNumber,
        char track,
        int unit,
        int lesson,
        int loIndex,
        string? suffix)
    {
        Prefix = prefix;
        Year = year;
        SubjectCode = subjectCode;
        Grade = grade;
        TermNumber = termNumber;
        Track = track;
        Unit = unit;
        Lesson = lesson;
        LoIndex = loIndex;
        Suffix = suffix;
    }

    public string? Prefix { get; }
    public int? Year { get; }
    public string SubjectCode { get; }
    public int Grade { get; }
    public int TermNumber { get; }
    public char Track { get; }
    public int Unit { get; }
    public int Lesson { get; }
    public int LoIndex { get; }
    public string? Suffix { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Prefix;
        yield return Year;
        yield return SubjectCode;
        yield return Grade;
        yield return TermNumber;
        yield return Track;
        yield return Unit;
        yield return Lesson;
        yield return LoIndex;
        yield return Suffix;
    }
}
