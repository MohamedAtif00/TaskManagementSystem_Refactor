using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class Test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjectives_Schemas_SchemaId",
                table: "LearningObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Steps_StepId",
                table: "Tasks");

            migrationBuilder.AlterColumn<int>(
                name: "StepId",
                table: "Tasks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningObjectives_Schemas_SchemaId",
                table: "LearningObjectives",
                column: "SchemaId",
                principalTable: "Schemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Steps_StepId",
                table: "Tasks",
                column: "StepId",
                principalTable: "Steps",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjectives_Schemas_SchemaId",
                table: "LearningObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Steps_StepId",
                table: "Tasks");

            migrationBuilder.AlterColumn<int>(
                name: "StepId",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningObjectives_Schemas_SchemaId",
                table: "LearningObjectives",
                column: "SchemaId",
                principalTable: "Schemas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Steps_StepId",
                table: "Tasks",
                column: "StepId",
                principalTable: "Steps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
