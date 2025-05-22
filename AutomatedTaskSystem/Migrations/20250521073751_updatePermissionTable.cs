using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class updatePermissionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Permissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "LeaveRequestId",
                table: "Opinions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PermissionId",
                table: "Opinions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opinions_PermissionId",
                table: "Opinions",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Opinions_Permissions_PermissionId",
                table: "Opinions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opinions_Permissions_PermissionId",
                table: "Opinions");

            migrationBuilder.DropIndex(
                name: "IX_Opinions_PermissionId",
                table: "Opinions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "PermissionId",
                table: "Opinions");

            migrationBuilder.AlterColumn<int>(
                name: "LeaveRequestId",
                table: "Opinions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
