using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class AddIsarchivedSprint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Sprints",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Sprints");
        }
    }
}
