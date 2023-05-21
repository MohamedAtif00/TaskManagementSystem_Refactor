using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class TaskTypeFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TaskBank_TypeId",
                table: "TaskBank",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskBank_Types_TypeId",
                table: "TaskBank",
                column: "TypeId",
                principalTable: "Types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskBank_Types_TypeId",
                table: "TaskBank");

            migrationBuilder.DropIndex(
                name: "IX_TaskBank_TypeId",
                table: "TaskBank");
        }
    }
}
