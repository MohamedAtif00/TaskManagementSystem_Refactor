using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class TaskType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reviewable",
                table: "TaskBank");

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "TaskBank",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Steps",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Types", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Types",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Creation" });

            migrationBuilder.InsertData(
                table: "Types",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "Comment" });

            migrationBuilder.InsertData(
                table: "Types",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "Review" });

            migrationBuilder.CreateIndex(
                name: "IX_Steps_TypeId",
                table: "Steps",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Types_TypeId",
                table: "Steps",
                column: "TypeId",
                principalTable: "Types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Types_TypeId",
                table: "Steps");

            migrationBuilder.DropTable(
                name: "Types");

            migrationBuilder.DropIndex(
                name: "IX_Steps_TypeId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "TaskBank");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Steps");

            migrationBuilder.AddColumn<bool>(
                name: "Reviewable",
                table: "TaskBank",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
