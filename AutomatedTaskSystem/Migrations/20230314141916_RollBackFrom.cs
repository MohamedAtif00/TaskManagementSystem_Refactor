using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class RollBackFrom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "FromId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2023, 3, 8, 12, 46, 47, 606, DateTimeKind.Local).AddTicks(7870));

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_FromId",
                table: "Tasks",
                column: "FromId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tasks_FromId",
                table: "Tasks",
                column: "FromId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tasks_FromId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_FromId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "FromId",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Steps",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2023, 3, 8, 12, 46, 47, 606, DateTimeKind.Local).AddTicks(7870),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Steps_GroupId",
                table: "Steps",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Groups_GroupId",
                table: "Steps",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
