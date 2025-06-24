using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class fixsprint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SprintLearningObjective_LearningObjectives_LearningObjectiveId",
                table: "SprintLearningObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_SprintLearningObjective_Sprints_SprintId",
                table: "SprintLearningObjective");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SprintLearningObjective",
                table: "SprintLearningObjective");

            migrationBuilder.RenameTable(
                name: "SprintLearningObjective",
                newName: "SprintLearningObjectives");

            migrationBuilder.RenameIndex(
                name: "IX_SprintLearningObjective_SprintId",
                table: "SprintLearningObjectives",
                newName: "IX_SprintLearningObjectives_SprintId");

            migrationBuilder.RenameIndex(
                name: "IX_SprintLearningObjective_LearningObjectiveId",
                table: "SprintLearningObjectives",
                newName: "IX_SprintLearningObjectives_LearningObjectiveId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SprintLearningObjectives",
                table: "SprintLearningObjectives",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SprintLearningObjectives_LearningObjectives_LearningObjectiveId",
                table: "SprintLearningObjectives",
                column: "LearningObjectiveId",
                principalTable: "LearningObjectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SprintLearningObjectives_Sprints_SprintId",
                table: "SprintLearningObjectives",
                column: "SprintId",
                principalTable: "Sprints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SprintLearningObjectives_LearningObjectives_LearningObjectiveId",
                table: "SprintLearningObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_SprintLearningObjectives_Sprints_SprintId",
                table: "SprintLearningObjectives");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SprintLearningObjectives",
                table: "SprintLearningObjectives");

            migrationBuilder.RenameTable(
                name: "SprintLearningObjectives",
                newName: "SprintLearningObjective");

            migrationBuilder.RenameIndex(
                name: "IX_SprintLearningObjectives_SprintId",
                table: "SprintLearningObjective",
                newName: "IX_SprintLearningObjective_SprintId");

            migrationBuilder.RenameIndex(
                name: "IX_SprintLearningObjectives_LearningObjectiveId",
                table: "SprintLearningObjective",
                newName: "IX_SprintLearningObjective_LearningObjectiveId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SprintLearningObjective",
                table: "SprintLearningObjective",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SprintLearningObjective_LearningObjectives_LearningObjectiveId",
                table: "SprintLearningObjective",
                column: "LearningObjectiveId",
                principalTable: "LearningObjectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SprintLearningObjective_Sprints_SprintId",
                table: "SprintLearningObjective",
                column: "SprintId",
                principalTable: "Sprints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
