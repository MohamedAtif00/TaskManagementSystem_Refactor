using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class IncludeGroupInStep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Steps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Steps_GroupId",
                table: "Steps",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Groups_GroupId",
                table: "Steps",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Groups_GroupId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Steps_GroupId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Steps");
        }
    }
}
