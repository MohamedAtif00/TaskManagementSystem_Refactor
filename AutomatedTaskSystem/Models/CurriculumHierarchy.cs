namespace AutomatedTaskSystem.Models;

/// <summary>
/// Fixed hierarchy: Season → Project → Term → Subject.
/// The root <see cref="Folder"/> is the Season; nested folders follow <see cref="FolderLevelNames"/>.
/// </summary>
public static class CurriculumHierarchy
{
    public const string RootLevelName = "Season";

    public static readonly string[] FolderLevelNames = ["Project", "Term", "Subject"];

    public const string DefaultLevelNamesJson = """["Project","Term","Subject"]""";
}
