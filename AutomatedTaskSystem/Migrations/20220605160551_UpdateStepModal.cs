using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class UpdateStepModal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Steps_NextStepId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Steps_NextStepId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "NextStepId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "PreviousStepId",
                table: "Steps");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Steps",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Steps");

            migrationBuilder.AddColumn<int>(
                name: "NextStepId",
                table: "Steps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousStepId",
                table: "Steps",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Steps_NextStepId",
                table: "Steps",
                column: "NextStepId",
                unique: true,
                filter: "[NextStepId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Steps_NextStepId",
                table: "Steps",
                column: "NextStepId",
                principalTable: "Steps",
                principalColumn: "Id");
        }
    }
}
