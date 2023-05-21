using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class EditCommentRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_LearningObjectives_LearningObjectiveId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "LearningObjectiveId",
                table: "Comments",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_LearningObjectiveId",
                table: "Comments",
                newName: "IX_Comments_TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Tasks_TaskId",
                table: "Comments",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Tasks_TaskId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Comments",
                newName: "LearningObjectiveId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_TaskId",
                table: "Comments",
                newName: "IX_Comments_LearningObjectiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_LearningObjectives_LearningObjectiveId",
                table: "Comments",
                column: "LearningObjectiveId",
                principalTable: "LearningObjectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
