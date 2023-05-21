using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class Seeder3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Schemas",
                columns: new[] { "Id", "Archived", "Description", "Duration", "GroupId", "IsStart", "Name", "Next", "Serial", "TypeId" },
                values: new object[,]
                {
                    { 1, false, "ID writing the storyboard", 3600, 1, true, "Storyboard writing", 2, 1, 1 },
                    { 2, false, "SME review the storyboard that the ID wrote", 3600, 2, false, "Storyboard Review", 3, 2, 1 },
                    { 3, false, "Design Proof Analysis", 3600, 4, false, "Analysis", 4, 3, 1 },
                    { 4, false, "Recording Voice Over", 3600, 5, false, "Voice Over", 5, 4, 1 },
                    { 5, false, "Graphic Design", 3600, 6, false, "Graphics", 6, 4, 1 },
                    { 6, false, "Voice Over Review", 3600, 4, false, "Review", null, 5, 1 },
                    { 7, false, "Graphic Design Review", 3600, 1, false, "Review", null, 6, 1 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Schemas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Schemas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Schemas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Schemas",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Schemas",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Schemas",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Schemas",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
