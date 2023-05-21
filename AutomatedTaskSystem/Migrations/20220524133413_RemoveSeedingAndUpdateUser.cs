using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class RemoveSeedingAndUpdateUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "LearningObjectivesTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "LearningObjectivesTypes",
                keyColumn: "Id",
                keyValue: 3);

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

            migrationBuilder.DeleteData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Groups",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "LearningObjectivesTypes",
                keyColumn: "Id",
                keyValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Groups",
                columns: new[] { "Id", "Archived", "Description", "Name" },
                values: new object[,]
                {
                    { 1, false, "Identity Designer", "ID" },
                    { 2, false, "Subject Matter Executive", "SME" },
                    { 3, false, "Project Manager", "PM" },
                    { 4, false, "Proofreader", "PR" },
                    { 5, false, "Voice Over", "VO" },
                    { 6, false, "Graphic Designer", "GD" }
                });

            migrationBuilder.InsertData(
                table: "LearningObjectivesTypes",
                columns: new[] { "Id", "Archived", "Description", "Name" },
                values: new object[,]
                {
                    { 1, false, "An interactive application with learning objective.", "Interactive" },
                    { 2, false, "A Live Video that explains the learning objective.", "Live Video" },
                    { 3, false, "A gamified learning objective.", "Game" }
                });

            migrationBuilder.InsertData(
                table: "Titles",
                columns: new[] { "Id", "Archived", "Description", "Name" },
                values: new object[,]
                {
                    { 1, false, "Active Project Manager", "Project Manager" },
                    { 2, false, "Designer with Advanced sets of knowledge", "Senior Designer" },
                    { 3, false, "A skilled graphic designer", "Junior Designer" }
                });

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
    }
}
