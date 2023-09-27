using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class AddWorkTimeDuration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TaskWorkTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Duration = table.Column<double>(type: "float", nullable: false),
                    EndReason = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskWorkTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskWorkTimes_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskWorkTimes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TaskId",
                table: "Comments",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkTimes_TaskId",
                table: "TaskWorkTimes",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkTimes_UserId",
                table: "TaskWorkTimes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Tasks_TaskId",
                table: "Comments",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");

            migrationBuilder.Sql(
                @"
				INSERT INTO TaskActivities (Type, TaskId, TaskSecondaryId, ActorOneId, ActorTwoId, TimeStamp, AdditionalInfo)
				SELECT 2, TaskId, NULL, ById, NULL, TimeStamp, NULL FROM Assignments WHERE ToId = ById;"
            );

            migrationBuilder.Sql(
                @"
				INSERT INTO TaskActivities (Type, TaskId, TaskSecondaryId, ActorOneId, ActorTwoId, TimeStamp, AdditionalInfo)
				SELECT 10, TaskId, NULL, ById, ToId, TimeStamp, NULL FROM Assignments WHERE NOT ToId = ById;"
            );

            migrationBuilder.Sql(
                @"
				INSERT INTO TaskWorkTimes (TaskId, UserId, StartDate, EndDate, Duration, EndReason)
				SELECT TaskId, UserId, StartDate, EndDate, 0, 0 FROM EndActivities WHERE EndActivityTypeId = 1;"
            );

            migrationBuilder.Sql(
                @"
				INSERT INTO TaskWorkTimes (TaskId, UserId, StartDate, EndDate, Duration, EndReason)
				SELECT TaskId, UserId, StartDate, EndDate, 0, 1 FROM EndActivities WHERE EndActivityTypeId = 2;"
            );

            migrationBuilder.Sql(
                @"
				INSERT INTO TaskWorkTimes (TaskId, UserId, StartDate, EndDate, Duration, EndReason)
				SELECT TaskId, UserId, StartDate, EndDate, 0, 2 FROM EndActivities WHERE EndActivityTypeId = 3;"
            );

            migrationBuilder.Sql(
                @"
				INSERT INTO TaskWorkTimes (TaskId, UserId, StartDate, EndDate, Duration, EndReason)
				SELECT TaskId, UserId, StartDate, EndDate, 0, 3 FROM EndActivities WHERE EndActivityTypeId = 4;"
            );

            migrationBuilder.Sql(
                @"
				INSERT INTO TaskWorkTimes (TaskId, UserId, StartDate, EndDate, Duration, EndReason)
				SELECT TaskId, UserId, StartDate, EndDate, 0, 4 FROM EndActivities WHERE EndActivityTypeId = 5;"
            );

            migrationBuilder.Sql(
                @"
				INSERT INTO TaskWorkTimes (TaskId, UserId, StartDate, EndDate, Duration, EndReason)
				SELECT TaskId, UserId, StartDate, EndDate, 0, 5 FROM EndActivities WHERE EndActivityTypeId = 6;"
            );

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "EndActivities");

            migrationBuilder.DropTable(
                name: "ActivityTypes");

            migrationBuilder.DropTable(
                name: "EndActivityTypes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Tasks_TaskId",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "TaskWorkTimes");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TaskId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "Comments");

            migrationBuilder.CreateTable(
                name: "ActivityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ById = table.Column<int>(type: "int", nullable: true),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    ToId = table.Column<int>(type: "int", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assignments_Users_ById",
                        column: x => x.ById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assignments_Users_ToId",
                        column: x => x.ToId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

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
                name: "Activities",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityTypeId = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.id);
                    table.ForeignKey(
                        name: "FK_Activities_ActivityTypes_ActivityTypeId",
                        column: x => x.ActivityTypeId,
                        principalTable: "ActivityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EndActivities",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EndActivityTypeId = table.Column<int>(type: "int", nullable: true),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                table: "ActivityTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Start" },
                    { 2, "Done" }
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
                    { 5, "Reassign" },
                    { 6, "Change of Schema" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ActivityTypeId",
                table: "Activities",
                column: "ActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_TaskId",
                table: "Activities",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_UserId",
                table: "Activities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ById",
                table: "Assignments",
                column: "ById");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_TaskId",
                table: "Assignments",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ToId",
                table: "Assignments",
                column: "ToId");

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
    }
}
