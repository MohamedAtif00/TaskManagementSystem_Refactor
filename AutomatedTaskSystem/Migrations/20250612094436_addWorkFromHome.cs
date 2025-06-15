using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class addWorkFromHome : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SectionGroups_Groups_GroupId",
                table: "SectionGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_SectionGroups_Sections_SectionId",
                table: "SectionGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Users_HeadId",
                table: "Sections");

            migrationBuilder.AddColumn<int>(
                name: "WorkFromHomeRequestId",
                table: "Opinions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkFromHomeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TeamleaderId = table.Column<int>(type: "int", nullable: true),
                    SectionheadId = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NoteForManager = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFromHomeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFromHomeRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Opinions_WorkFromHomeRequestId",
                table: "Opinions",
                column: "WorkFromHomeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFromHomeRequests_UserId",
                table: "WorkFromHomeRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Opinions_WorkFromHomeRequests_WorkFromHomeRequestId",
                table: "Opinions",
                column: "WorkFromHomeRequestId",
                principalTable: "WorkFromHomeRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SectionGroups_Groups_GroupId",
                table: "SectionGroups",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SectionGroups_Sections_SectionId",
                table: "SectionGroups",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Users_HeadId",
                table: "Sections",
                column: "HeadId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opinions_WorkFromHomeRequests_WorkFromHomeRequestId",
                table: "Opinions");

            migrationBuilder.DropForeignKey(
                name: "FK_SectionGroups_Groups_GroupId",
                table: "SectionGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_SectionGroups_Sections_SectionId",
                table: "SectionGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Users_HeadId",
                table: "Sections");

            migrationBuilder.DropTable(
                name: "WorkFromHomeRequests");

            migrationBuilder.DropIndex(
                name: "IX_Opinions_WorkFromHomeRequestId",
                table: "Opinions");

            migrationBuilder.DropColumn(
                name: "WorkFromHomeRequestId",
                table: "Opinions");

            migrationBuilder.AddForeignKey(
                name: "FK_SectionGroups_Groups_GroupId",
                table: "SectionGroups",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SectionGroups_Sections_SectionId",
                table: "SectionGroups",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Users_HeadId",
                table: "Sections",
                column: "HeadId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
