using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class ProjectYearAndTerm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Term",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "YearId",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Years",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Years", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Years",
                columns: new[] { "Id", "Active", "Number" },
                values: new object[,]
                {
                    { 1, true, "2020" },
                    { 2, true, "2021" },
                    { 3, true, "2022" },
                    { 4, true, "2023" },
                    { 5, true, "2024" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_YearId",
                table: "Projects",
                column: "YearId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Years_YearId",
                table: "Projects",
                column: "YearId",
                principalTable: "Years",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Years_YearId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "Years");

            migrationBuilder.DropIndex(
                name: "IX_Projects_YearId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Term",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "YearId",
                table: "Projects");
        }
    }
}
