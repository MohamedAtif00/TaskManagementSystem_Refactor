using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    /// <inheritdoc />
    public partial class ProjectLevelNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LevelNamesJson",
                table: "FolderProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [FolderProjects]
                SET [LevelNamesJson] = N'["Years","Terms","Subjects"]'
                WHERE [LevelNamesJson] IS NULL;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LevelNamesJson",
                table: "FolderProjects");
        }
    }
}
