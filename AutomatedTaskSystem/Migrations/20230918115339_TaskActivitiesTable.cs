using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
	public partial class TaskActivitiesTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "TaskActivities",
				columns: table =>
					new
					{
						Id = table
							.Column<int>(type: "int", nullable: false)
							.Annotation("SqlServer:Identity", "1, 1"),
						Type = table.Column<int>(type: "int", nullable: false),
						TaskId = table.Column<int>(type: "int", nullable: false),
						TaskSecondaryId = table.Column<int>(type: "int", nullable: true),
						ActorOneId = table.Column<int>(type: "int", nullable: true),
						ActorTwoId = table.Column<int>(type: "int", nullable: true),
						TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
						AdditionalInfo = table.Column<string>(type: "nvarchar(max)", nullable: true)
					},
				constraints: table =>
				{
					table.PrimaryKey("PK_TaskActivities", x => x.Id);
					table.ForeignKey(
						name: "FK_TaskActivities_Tasks_TaskId",
						column: x => x.TaskId,
						principalTable: "Tasks",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade
					);
					table.ForeignKey(
						name: "FK_TaskActivities_Tasks_TaskSecondaryId",
						column: x => x.TaskSecondaryId,
						principalTable: "Tasks",
						principalColumn: "Id"
					);
					table.ForeignKey(
						name: "FK_TaskActivities_Users_ActorOneId",
						column: x => x.ActorOneId,
						principalTable: "Users",
						principalColumn: "Id"
					);
					table.ForeignKey(
						name: "FK_TaskActivities_Users_ActorTwoId",
						column: x => x.ActorTwoId,
						principalTable: "Users",
						principalColumn: "Id"
					);
				}
			);

			migrationBuilder.CreateIndex(
				name: "IX_TaskActivities_ActorOneId",
				table: "TaskActivities",
				column: "ActorOneId"
			);

			migrationBuilder.CreateIndex(
				name: "IX_TaskActivities_ActorTwoId",
				table: "TaskActivities",
				column: "ActorTwoId"
			);

			migrationBuilder.CreateIndex(
				name: "IX_TaskActivities_TaskId",
				table: "TaskActivities",
				column: "TaskId"
			);

			migrationBuilder.CreateIndex(
				name: "IX_TaskActivities_TaskSecondaryId",
				table: "TaskActivities",
				column: "TaskSecondaryId"
			);

			migrationBuilder.Sql(
				@"
				INSERT INTO TaskActivities (Type, TaskId, TaskSecondaryId, ActorOneId, ActorTwoId, TimeStamp, AdditionalInfo)
				SELECT 3, TaskId, NULL, UserId, NULL, StartDate, NULL FROM EndActivities;

				INSERT INTO TaskActivities (Type, TaskId, TaskSecondaryId, ActorOneId, ActorTwoId, TimeStamp, AdditionalInfo)
				SELECT 8, TaskId, NULL, UserId, NULL, EndDate, NULL FROM EndActivities WHERE EndActivityTypeId = 1;

				INSERT INTO TaskActivities (Type, TaskId, TaskSecondaryId, ActorOneId, ActorTwoId, TimeStamp, AdditionalInfo)
				SELECT 6, TaskId, NULL, UserId, NULL, EndDate, NULL FROM EndActivities WHERE EndActivityTypeId = 2;

				INSERT INTO TaskActivities (Type, TaskId, TaskSecondaryId, ActorOneId, ActorTwoId, TimeStamp, AdditionalInfo)
				SELECT 4, TaskId, NULL, UserId, NULL, EndDate, NULL FROM EndActivities WHERE EndActivityTypeId = 3;

				INSERT INTO TaskActivities (Type, TaskId, TaskSecondaryId, ActorOneId, ActorTwoId, TimeStamp, AdditionalInfo)
				SELECT 1, Id, NULL, NULL, NULL, CreatedAt, NULL FROM Tasks;
			");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(name: "TaskActivities");
		}
	}
}
