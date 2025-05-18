using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class addUserChangesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserChanges_Users_ChangedByUserId",
                table: "UserChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_UserChanges_Users_UserId",
                table: "UserChanges");

            migrationBuilder.AddForeignKey(
                name: "FK_UserChanges_Users_ChangedByUserId",
                table: "UserChanges",
                column: "ChangedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserChanges_Users_UserId",
                table: "UserChanges",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserChanges_Users_ChangedByUserId",
                table: "UserChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_UserChanges_Users_UserId",
                table: "UserChanges");

            migrationBuilder.AddForeignKey(
                name: "FK_UserChanges_Users_ChangedByUserId",
                table: "UserChanges",
                column: "ChangedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserChanges_Users_UserId",
                table: "UserChanges",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
