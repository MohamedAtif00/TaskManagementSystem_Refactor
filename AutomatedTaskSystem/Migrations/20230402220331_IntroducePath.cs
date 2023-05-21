using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class IntroducePath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Paths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: true),
                    StepId = table.Column<int>(type: "int", nullable: false),
                    NextStepId = table.Column<int>(type: "int", nullable: false),
                    LearningObjectiveId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paths_LearningObjectives_LearningObjectiveId",
                        column: x => x.LearningObjectiveId,
                        principalTable: "LearningObjectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Paths_Steps_NextStepId",
                        column: x => x.NextStepId,
                        principalTable: "Steps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Paths_Steps_StepId",
                        column: x => x.StepId,
                        principalTable: "Steps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Paths_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Paths_LearningObjectiveId",
                table: "Paths",
                column: "LearningObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_Paths_NextStepId",
                table: "Paths",
                column: "NextStepId");

            migrationBuilder.CreateIndex(
                name: "IX_Paths_StepId",
                table: "Paths",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_Paths_TaskId",
                table: "Paths",
                column: "TaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Paths");
        }
    }
}
