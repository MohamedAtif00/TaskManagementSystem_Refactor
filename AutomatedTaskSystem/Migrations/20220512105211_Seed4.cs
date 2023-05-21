using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class Seed4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Titles",
                columns: new[] { "Id", "Archived", "Description", "Name" },
                values: new object[] { 1, false, "Active Project Manager", "Project Manager" });

            migrationBuilder.InsertData(
                table: "Titles",
                columns: new[] { "Id", "Archived", "Description", "Name" },
                values: new object[] { 2, false, "Designer with Advanced sets of knowledge", "Senior Designer" });

            migrationBuilder.InsertData(
                table: "Titles",
                columns: new[] { "Id", "Archived", "Description", "Name" },
                values: new object[] { 3, false, "A skilled graphic designer", "Junior Designer" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
