using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class ReworkActivities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimeStamp",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 24, 13, 16, 53, 611, DateTimeKind.Local).AddTicks(2640));

            migrationBuilder.CreateTable(
                name: "EndActivityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndActivityTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EndActivities",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndActivityTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndActivities", x => x.id);
                    table.ForeignKey(
                        name: "FK_EndActivities_EndActivityTypes_EndActivityTypeId",
                        column: x => x.EndActivityTypeId,
                        principalTable: "EndActivityTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EndActivities_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EndActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EndActivityTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Flag" },
                    { 2, "Pause" },
                    { 3, "Complete" },
                    { 4, "Session" },
                    { 5, "Reassign" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EndActivities_EndActivityTypeId",
                table: "EndActivities",
                column: "EndActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EndActivities_TaskId",
                table: "EndActivities",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_EndActivities_UserId",
                table: "EndActivities",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EndActivities");

            migrationBuilder.DropTable(
                name: "EndActivityTypes");

            migrationBuilder.DropColumn(
                name: "TimeStamp",
                table: "Assignments");
        }
    }
}
