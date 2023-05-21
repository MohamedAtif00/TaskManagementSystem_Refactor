using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class PauseFuntion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Pause",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 24, 16, 30, 23, 150, DateTimeKind.Local).AddTicks(9870),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2023, 1, 24, 13, 16, 53, 611, DateTimeKind.Local).AddTicks(2640));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pause",
                table: "Tasks");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 24, 13, 16, 53, 611, DateTimeKind.Local).AddTicks(2640),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2023, 1, 24, 16, 30, 23, 150, DateTimeKind.Local).AddTicks(9870));
        }
    }
}
