using Microsoft.EntityFrameworkCore;

namespace AutomatedTaskSystem.Data;

/// <summary>
/// Idempotent SQL repair when the SubjectHierarchy migration did not finish
/// (e.g. Projects table still exists instead of Subjects).
/// </summary>
public static class SubjectSchemaRepair
{
    private const string RepairSql = """
        IF OBJECT_ID(N'dbo.Projects', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.Subjects', N'U') IS NULL
            EXEC sp_rename N'dbo.Projects', N'Subjects';

        IF COL_LENGTH(N'dbo.Units', N'ProjectId') IS NOT NULL AND COL_LENGTH(N'dbo.Units', N'SubjectId') IS NULL
            EXEC sp_rename N'dbo.Units.ProjectId', N'SubjectId', N'COLUMN';

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Units_ProjectId' AND object_id = OBJECT_ID(N'dbo.Units'))
            EXEC sp_rename N'dbo.Units.IX_Units_ProjectId', N'IX_Units_SubjectId', N'INDEX';

        IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Units_Projects_ProjectId')
            ALTER TABLE dbo.Units DROP CONSTRAINT FK_Units_Projects_ProjectId;

        IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.Units', N'SubjectId') IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Units_Subjects_SubjectId')
            ALTER TABLE dbo.Units ADD CONSTRAINT FK_Units_Subjects_SubjectId
                FOREIGN KEY (SubjectId) REFERENCES dbo.Subjects(Id) ON DELETE CASCADE;

        IF OBJECT_ID(N'dbo.ProjectUser', N'U') IS NOT NULL
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProjectUser_Projects_ProjectsId')
                ALTER TABLE dbo.ProjectUser DROP CONSTRAINT FK_ProjectUser_Projects_ProjectsId;
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProjectUser_Users_UsersId')
                ALTER TABLE dbo.ProjectUser DROP CONSTRAINT FK_ProjectUser_Users_UsersId;
        END

        IF OBJECT_ID(N'dbo.ProjectUser', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.SubjectUser', N'U') IS NULL
            EXEC sp_rename N'dbo.ProjectUser', N'SubjectUser';

        IF OBJECT_ID(N'dbo.SubjectUser', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.SubjectUser', N'ProjectsId') IS NOT NULL
           AND COL_LENGTH(N'dbo.SubjectUser', N'SubjectsId') IS NULL
            EXEC sp_rename N'dbo.SubjectUser.ProjectsId', N'SubjectsId', N'COLUMN';

        IF OBJECT_ID(N'dbo.SubjectUser', N'U') IS NOT NULL
           AND OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SubjectUser_Subjects_SubjectsId')
            ALTER TABLE dbo.SubjectUser ADD CONSTRAINT FK_SubjectUser_Subjects_SubjectsId
                FOREIGN KEY (SubjectsId) REFERENCES dbo.Subjects(Id) ON DELETE CASCADE;

        IF OBJECT_ID(N'dbo.SubjectUser', N'U') IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SubjectUser_Users_UsersId')
            ALTER TABLE dbo.SubjectUser ADD CONSTRAINT FK_SubjectUser_Users_UsersId
                FOREIGN KEY (UsersId) REFERENCES dbo.Users(Id) ON DELETE NO ACTION;

        IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.Subjects', N'SubjectGroupId') IS NULL
           AND COL_LENGTH(N'dbo.Subjects', N'FolderId') IS NOT NULL
            ALTER TABLE dbo.Subjects ADD SubjectGroupId INT NULL;

        IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.Subjects', N'SubjectGroupId') IS NULL
           AND COL_LENGTH(N'dbo.Subjects', N'FolderId') IS NULL
            ALTER TABLE dbo.Subjects ADD SubjectGroupId INT NOT NULL CONSTRAINT DF_Subjects_SubjectGroupId DEFAULT 0;

        IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.Subjects', N'SubjectGroupId') IS NOT NULL
           AND OBJECT_ID(N'dbo.SubjectGroups', N'U') IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Subjects_SubjectGroups_SubjectGroupId')
            ALTER TABLE dbo.Subjects ADD CONSTRAINT FK_Subjects_SubjectGroups_SubjectGroupId
                FOREIGN KEY (SubjectGroupId) REFERENCES dbo.SubjectGroups(Id) ON DELETE NO ACTION;

        IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.Subjects', N'SubjectGroupId') IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Subjects_SubjectGroupId' AND object_id = OBJECT_ID(N'dbo.Subjects'))
            CREATE INDEX IX_Subjects_SubjectGroupId ON dbo.Subjects(SubjectGroupId);

        IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.Subjects', N'ArchivedWithFolder') IS NULL
            ALTER TABLE dbo.Subjects ADD ArchivedWithFolder BIT NOT NULL CONSTRAINT DF_Subjects_ArchivedWithFolder DEFAULT 0;

        IF OBJECT_ID(N'dbo.AcademicYears', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.AcademicYears', N'Archived') IS NULL
            ALTER TABLE dbo.AcademicYears ADD Archived BIT NOT NULL CONSTRAINT DF_AcademicYears_Archived DEFAULT 0;

        IF OBJECT_ID(N'dbo.CurriculumProjects', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.CurriculumProjects', N'Archived') IS NULL
            ALTER TABLE dbo.CurriculumProjects ADD Archived BIT NOT NULL CONSTRAINT DF_CurriculumProjects_Archived DEFAULT 0;

        IF OBJECT_ID(N'dbo.CurriculumTerms', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.CurriculumTerms', N'Archived') IS NULL
            ALTER TABLE dbo.CurriculumTerms ADD Archived BIT NOT NULL CONSTRAINT DF_CurriculumTerms_Archived DEFAULT 0;

        IF OBJECT_ID(N'dbo.SubjectGroups', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.SubjectGroups', N'Archived') IS NULL
            ALTER TABLE dbo.SubjectGroups ADD Archived BIT NOT NULL CONSTRAINT DF_SubjectGroups_Archived DEFAULT 0;
        """;

    public static async Task ApplyAsync(DataContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.ExecuteSqlRawAsync(RepairSql, cancellationToken);
    }
}
