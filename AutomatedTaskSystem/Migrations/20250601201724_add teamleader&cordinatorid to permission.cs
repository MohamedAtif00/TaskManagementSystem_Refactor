using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class addteamleadercordinatoridtopermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SectionheadId",
                table: "Permissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamleaderId",
                table: "Permissions",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectionheadId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "TeamleaderId",
                table: "Permissions");
        }
    }
}
