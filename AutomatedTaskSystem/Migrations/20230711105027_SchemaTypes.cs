using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class SchemaTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Schemas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SchemaTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schemas_TypeId",
                table: "Schemas",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schemas_SchemaTypes_TypeId",
                table: "Schemas",
                column: "TypeId",
                principalTable: "SchemaTypes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schemas_SchemaTypes_TypeId",
                table: "Schemas");

            migrationBuilder.DropTable(
                name: "SchemaTypes");

            migrationBuilder.DropIndex(
                name: "IX_Schemas_TypeId",
                table: "Schemas");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Schemas");
        }
    }
}
