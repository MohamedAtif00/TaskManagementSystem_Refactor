using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class RemoveTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskBank_Types_TypeId",
                table: "TaskBank");

            migrationBuilder.DropTable(
                name: "Types");

            migrationBuilder.DropIndex(
                name: "IX_TaskBank_TypeId",
                table: "TaskBank");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TaskBank",
                type: "int",
                nullable: false,
                defaultValue: 0);

			migrationBuilder.Sql(@"
					UPDATE TaskBank SET Type = 0 WHERE TypeId = 1;
					UPDATE TaskBank SET Type = 1 WHERE TypeId = 3;
					");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "TaskBank");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "TaskBank");

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "TaskBank",
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
                values: new object[] { 3, "Review" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskBank_TypeId",
                table: "TaskBank",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskBank_Types_TypeId",
                table: "TaskBank",
                column: "TypeId",
                principalTable: "Types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
