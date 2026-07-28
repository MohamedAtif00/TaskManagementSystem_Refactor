namespace AutomatedTaskSystem.Models;



/// <summary>

/// Fixed hierarchy: Year → Project → Term → Subject Group → Subject records.

/// </summary>

public static class CurriculumHierarchy

{

    public const string RootLevelName = "Year";



    public static readonly string[] ChildLevelNames = ["Project", "Term", "Subject Group"];



    public static string[] AllLevelNames => [RootLevelName, ..ChildLevelNames];



    public static string ResolveLevelName(int depth)

    {

        if (depth <= 0)

            return RootLevelName;



        var index = depth - 1;

        return index < ChildLevelNames.Length

            ? ChildLevelNames[index]

            : ChildLevelNames[^1];

    }

}


