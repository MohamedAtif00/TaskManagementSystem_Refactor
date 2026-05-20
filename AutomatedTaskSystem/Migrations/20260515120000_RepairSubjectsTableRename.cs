using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    /// <inheritdoc />
    public partial class RepairSubjectsTableRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
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
                   AND COL_LENGTH(N'dbo.Subjects', N'TermId') IS NOT NULL
                   AND OBJECT_ID(N'dbo.Terms', N'U') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Subjects_Terms_TermId')
                    ALTER TABLE dbo.Subjects ADD CONSTRAINT FK_Subjects_Terms_TermId
                        FOREIGN KEY (TermId) REFERENCES dbo.Terms(Id) ON DELETE NO ACTION;

                IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Subjects_TermId' AND object_id = OBJECT_ID(N'dbo.Subjects'))
                    CREATE INDEX IX_Subjects_TermId ON dbo.Subjects(TermId);
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Repair migration — no down.
        }
    }
}
