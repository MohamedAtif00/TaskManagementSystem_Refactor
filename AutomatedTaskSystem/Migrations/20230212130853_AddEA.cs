using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class AddEA : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2023, 2, 12, 15, 8, 53, 414, DateTimeKind.Local).AddTicks(1150),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2023, 1, 29, 13, 26, 28, 905, DateTimeKind.Local).AddTicks(1390));

            migrationBuilder.InsertData(
                table: "EndActivityTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 6, "Change of Schema" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EndActivityTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 29, 13, 26, 28, 905, DateTimeKind.Local).AddTicks(1390),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2023, 2, 12, 15, 8, 53, 414, DateTimeKind.Local).AddTicks(1150));
        }
    }
}
