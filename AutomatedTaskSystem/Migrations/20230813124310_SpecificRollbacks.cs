using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class SpecificRollbacks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RSteps",
                columns: table => new
                {
                    FromId = table.Column<int>(type: "int", nullable: false),
                    RollbacksId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RSteps", x => new { x.FromId, x.RollbacksId });
                    table.ForeignKey(
                        name: "FK_RSteps_Steps_FromId",
                        column: x => x.FromId,
                        principalTable: "Steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RSteps_Steps_RollbacksId",
                        column: x => x.RollbacksId,
                        principalTable: "Steps",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RSteps_RollbacksId",
                table: "RSteps",
                column: "RollbacksId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RSteps");
        }
    }
}
