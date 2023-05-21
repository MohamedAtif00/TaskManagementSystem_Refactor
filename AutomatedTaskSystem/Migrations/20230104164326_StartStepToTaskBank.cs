using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class StartStepToTaskBank : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskBankId",
                table: "Steps",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Steps_TaskBankId",
                table: "Steps",
                column: "TaskBankId");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_TaskBank_TaskBankId",
                table: "Steps",
                column: "TaskBankId",
                principalTable: "TaskBank",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_TaskBank_TaskBankId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Steps_TaskBankId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "TaskBankId",
                table: "Steps");
        }
    }
}
