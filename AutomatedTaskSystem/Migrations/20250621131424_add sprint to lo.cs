using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class addsprinttolo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SprintId",
                table: "LearningObjectives",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningObjectives_SprintId",
                table: "LearningObjectives",
                column: "SprintId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningObjectives_Sprints_SprintId",
                table: "LearningObjectives",
                column: "SprintId",
                principalTable: "Sprints",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjectives_Sprints_SprintId",
                table: "LearningObjectives");

            migrationBuilder.DropIndex(
                name: "IX_LearningObjectives_SprintId",
                table: "LearningObjectives");

            migrationBuilder.DropColumn(
                name: "SprintId",
                table: "LearningObjectives");
        }
    }
}
