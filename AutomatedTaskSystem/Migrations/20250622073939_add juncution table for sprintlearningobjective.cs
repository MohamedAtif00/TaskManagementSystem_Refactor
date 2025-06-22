using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class addjuncutiontableforsprintlearningobjective : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjectives_Sprints_SprintId",
                table: "LearningObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Sprints_SprintId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_SprintId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_LearningObjectives_SprintId",
                table: "LearningObjectives");

            migrationBuilder.DropColumn(
                name: "SprintId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "SprintId",
                table: "LearningObjectives");

            migrationBuilder.CreateTable(
                name: "SprintLearningObjective",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SprintId = table.Column<int>(type: "int", nullable: false),
                    LearningObjectiveId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SprintLearningObjective", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SprintLearningObjective_LearningObjectives_LearningObjectiveId",
                        column: x => x.LearningObjectiveId,
                        principalTable: "LearningObjectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SprintLearningObjective_Sprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "Sprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SprintLearningObjective_LearningObjectiveId",
                table: "SprintLearningObjective",
                column: "LearningObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_SprintLearningObjective_SprintId",
                table: "SprintLearningObjective",
                column: "SprintId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SprintLearningObjective");

            migrationBuilder.AddColumn<int>(
                name: "SprintId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SprintId",
                table: "LearningObjectives",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SprintId",
                table: "Tasks",
                column: "SprintId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Sprints_SprintId",
                table: "Tasks",
                column: "SprintId",
                principalTable: "Sprints",
                principalColumn: "Id");
        }
    }
}
