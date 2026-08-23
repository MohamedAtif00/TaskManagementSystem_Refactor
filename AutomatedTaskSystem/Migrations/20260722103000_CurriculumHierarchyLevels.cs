using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    /// <inheritdoc />
    public partial class CurriculumHierarchyLevels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [FolderProjects]
                SET [LevelNamesJson] = N'["Season","Term","Family"]'
                WHERE [LevelNamesJson] IS NULL
                   OR [LevelNamesJson] = N'["Years","Terms","Subjects"]';
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [FolderProjects]
                SET [LevelNamesJson] = N'["Years","Terms","Subjects"]'
                WHERE [LevelNamesJson] = N'["Season","Term","Family"]';
                """
            );
        }
    }
}
