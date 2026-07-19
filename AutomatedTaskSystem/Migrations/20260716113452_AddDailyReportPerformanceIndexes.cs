using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyReportPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rollbacks_TaskId",
                table: "Rollbacks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Archived_GroupId_CreatedAt",
                table: "Tasks",
                columns: new[] { "Archived", "GroupId", "CreatedAt" })
                .Annotation("SqlServer:Include", new[] { "LearningObjectiveId", "StepId", "UserId", "Status", "IsRollback", "Priority", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskActivities_Type_TimeStamp_TaskId",
                table: "TaskActivities",
                columns: new[] { "Type", "TimeStamp" })
                .Annotation("SqlServer:Include", new[] { "TaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_Rollbacks_TaskId_Id",
                table: "Rollbacks",
                columns: new[] { "TaskId", "Id" },
                descending: new[] { false, true })
                .Annotation("SqlServer:Include", new[] { "Clarification", "ToTaskId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_Archived_GroupId_CreatedAt",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_TaskActivities_Type_TimeStamp_TaskId",
                table: "TaskActivities");

            migrationBuilder.DropIndex(
                name: "IX_Rollbacks_TaskId_Id",
                table: "Rollbacks");

            migrationBuilder.CreateIndex(
                name: "IX_Rollbacks_TaskId",
                table: "Rollbacks",
                column: "TaskId");
        }
    }
}
