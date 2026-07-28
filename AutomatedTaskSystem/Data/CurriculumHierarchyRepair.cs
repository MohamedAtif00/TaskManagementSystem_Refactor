using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Data;

/// <summary>
/// Idempotent repair when legacy-term migration created a duplicate project
/// named after the academic year instead of nesting terms under the real project.
/// </summary>
public static class CurriculumHierarchyRepair
{
    private const string RepairSql = """
        IF OBJECT_ID(N'dbo.AcademicYears', N'U') IS NOT NULL
           AND OBJECT_ID(N'dbo.CurriculumProjects', N'U') IS NOT NULL
           AND OBJECT_ID(N'dbo.CurriculumTerms', N'U') IS NOT NULL
        BEGIN
            UPDATE ct
            SET ct.[ProjectId] = realProj.[Id]
            FROM [CurriculumTerms] ct
            INNER JOIN [CurriculumProjects] dupProj ON dupProj.[Id] = ct.[ProjectId]
            INNER JOIN [AcademicYears] ay ON ay.[Id] = dupProj.[YearId] AND dupProj.[Name] = ay.[Name]
            INNER JOIN [CurriculumProjects] realProj ON realProj.[YearId] = ay.[Id] AND realProj.[Name] <> ay.[Name]
            WHERE realProj.[Id] = (
                SELECT MIN(cp.[Id])
                FROM [CurriculumProjects] cp
                WHERE cp.[YearId] = ay.[Id] AND cp.[Name] <> ay.[Name]);

            DELETE cp
            FROM [CurriculumProjects] cp
            INNER JOIN [AcademicYears] ay ON ay.[Id] = cp.[YearId] AND cp.[Name] = ay.[Name]
            WHERE NOT EXISTS (SELECT 1 FROM [CurriculumTerms] ct WHERE ct.[ProjectId] = cp.[Id]);
        END
        """;

    public static async Task ApplyAsync(DataContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.ExecuteSqlRawAsync(RepairSql, cancellationToken);
    }
}
