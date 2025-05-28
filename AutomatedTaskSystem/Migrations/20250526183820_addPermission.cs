using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class addPermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<int>(
            //   name: "Permission_MAX",
            //   table: "Users",
            //   type: "int",
            //   nullable: false,
            //   defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "Permission",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(name: "Permission_MAX", table: "Users");
            migrationBuilder.DropColumn(name: "Permission", table: "Users");
        }

    }
}
