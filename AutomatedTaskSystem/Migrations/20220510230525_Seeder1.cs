using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class Seeder1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LearningObjectivesTypes",
                columns: new[] { "Id", "Archived", "Description", "Name" },
                values: new object[] { 1, false, "An interactive application with learning objective.", "Interactive" });

            migrationBuilder.InsertData(
                table: "LearningObjectivesTypes",
                columns: new[] { "Id", "Archived", "Description", "Name" },
                values: new object[] { 2, false, "A Live Video that explains the learning objective.", "Live Video" });

            migrationBuilder.InsertData(
                table: "LearningObjectivesTypes",
                columns: new[] { "Id", "Archived", "Description", "Name" },
                values: new object[] { 3, false, "A gamified learning objective.", "Game" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LearningObjectivesTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "LearningObjectivesTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "LearningObjectivesTypes",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
