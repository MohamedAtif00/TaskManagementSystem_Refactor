using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class MinorRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjectives_LeaningObjectivesTypes_TypeId",
                table: "LearningObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_Schemas_LeaningObjectivesTypes_TypeId",
                table: "Schemas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaningObjectivesTypes",
                table: "LeaningObjectivesTypes");

            migrationBuilder.RenameTable(
                name: "LeaningObjectivesTypes",
                newName: "LearningObjectivesTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningObjectivesTypes",
                table: "LearningObjectivesTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningObjectives_LearningObjectivesTypes_TypeId",
                table: "LearningObjectives",
                column: "TypeId",
                principalTable: "LearningObjectivesTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schemas_LearningObjectivesTypes_TypeId",
                table: "Schemas",
                column: "TypeId",
                principalTable: "LearningObjectivesTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjectives_LearningObjectivesTypes_TypeId",
                table: "LearningObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_Schemas_LearningObjectivesTypes_TypeId",
                table: "Schemas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningObjectivesTypes",
                table: "LearningObjectivesTypes");

            migrationBuilder.RenameTable(
                name: "LearningObjectivesTypes",
                newName: "LeaningObjectivesTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaningObjectivesTypes",
                table: "LeaningObjectivesTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningObjectives_LeaningObjectivesTypes_TypeId",
                table: "LearningObjectives",
                column: "TypeId",
                principalTable: "LeaningObjectivesTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schemas_LeaningObjectivesTypes_TypeId",
                table: "Schemas",
                column: "TypeId",
                principalTable: "LeaningObjectivesTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
