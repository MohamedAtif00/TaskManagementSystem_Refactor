using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    /// <inheritdoc />
    public partial class RenameFolderLevelsProjectSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [FolderProjects]
                SET [LevelNamesJson] = N'["Project","Term","Subject"]'
                WHERE [LevelNamesJson] IS NULL
                   OR [LevelNamesJson] = N'["Years","Terms","Subjects"]'
                   OR [LevelNamesJson] = N'["Season","Term","Family"]'
                   OR [LevelNamesJson] = N'["Program","Term","Family"]';
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [FolderProjects]
                SET [LevelNamesJson] = N'["Program","Term","Family"]'
                WHERE [LevelNamesJson] = N'["Project","Term","Subject"]';
                """
            );
        }
    }
}
