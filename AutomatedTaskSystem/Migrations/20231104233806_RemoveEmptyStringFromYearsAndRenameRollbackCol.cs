using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class RemoveEmptyStringFromYearsAndRenameRollbackCol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rollbacks_Tasks_FromTaskId",
                table: "Rollbacks");

            migrationBuilder.RenameColumn(
                name: "FromTaskId",
                table: "Rollbacks",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Rollbacks_FromTaskId",
                table: "Rollbacks",
                newName: "IX_Rollbacks_TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rollbacks_Tasks_TaskId",
                table: "Rollbacks",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rollbacks_Tasks_TaskId",
                table: "Rollbacks");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Rollbacks",
                newName: "FromTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Rollbacks_TaskId",
                table: "Rollbacks",
                newName: "IX_Rollbacks_FromTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rollbacks_Tasks_FromTaskId",
                table: "Rollbacks",
                column: "FromTaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
