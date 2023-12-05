using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class AddTaskRollbackRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rollbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromTaskId = table.Column<int>(type: "int", nullable: false),
                    ToTaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Clarification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rollbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rollbacks_Tasks_FromTaskId",
                        column: x => x.FromTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rollbacks_Tasks_ToTaskId",
                        column: x => x.ToTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rollbacks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RollbackIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RollbackId = table.Column<int>(type: "int", nullable: false),
                    StepId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RollbackIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RollbackIssues_Rollbacks_RollbackId",
                        column: x => x.RollbackId,
                        principalTable: "Rollbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RollbackIssues_Steps_StepId",
                        column: x => x.StepId,
                        principalTable: "Steps",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RollbackIssues_RollbackId",
                table: "RollbackIssues",
                column: "RollbackId");

            migrationBuilder.CreateIndex(
                name: "IX_RollbackIssues_StepId",
                table: "RollbackIssues",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_Rollbacks_FromTaskId",
                table: "Rollbacks",
                column: "FromTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Rollbacks_ToTaskId",
                table: "Rollbacks",
                column: "ToTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Rollbacks_UserId",
                table: "Rollbacks",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RollbackIssues");

            migrationBuilder.DropTable(
                name: "Rollbacks");
        }
    }
}
