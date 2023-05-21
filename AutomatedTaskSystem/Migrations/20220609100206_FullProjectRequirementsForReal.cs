using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class FullProjectRequirementsForReal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjective_Lesson_LessonId",
                table: "LearningObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjective_Schemas_SchemaId",
                table: "LearningObjective");

            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_Unit_UnitId",
                table: "Lesson");

            migrationBuilder.DropForeignKey(
                name: "FK_Unit_Projects_ProjectId",
                table: "Unit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Unit",
                table: "Unit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lesson",
                table: "Lesson");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningObjective",
                table: "LearningObjective");

            migrationBuilder.RenameTable(
                name: "Unit",
                newName: "Units");

            migrationBuilder.RenameTable(
                name: "Lesson",
                newName: "Lessons");

            migrationBuilder.RenameTable(
                name: "LearningObjective",
                newName: "LearningObjectives");

            migrationBuilder.RenameIndex(
                name: "IX_Unit_ProjectId",
                table: "Units",
                newName: "IX_Units_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Lesson_UnitId",
                table: "Lessons",
                newName: "IX_Lessons_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningObjective_SchemaId",
                table: "LearningObjectives",
                newName: "IX_LearningObjectives_SchemaId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningObjective_LessonId",
                table: "LearningObjectives",
                newName: "IX_LearningObjectives_LessonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Units",
                table: "Units",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningObjectives",
                table: "LearningObjectives",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningObjectives_Lessons_LessonId",
                table: "LearningObjectives",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningObjectives_Schemas_SchemaId",
                table: "LearningObjectives",
                column: "SchemaId",
                principalTable: "Schemas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Units_UnitId",
                table: "Lessons",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Projects_ProjectId",
                table: "Units",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjectives_Lessons_LessonId",
                table: "LearningObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjectives_Schemas_SchemaId",
                table: "LearningObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Units_UnitId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Projects_ProjectId",
                table: "Units");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Units",
                table: "Units");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningObjectives",
                table: "LearningObjectives");

            migrationBuilder.RenameTable(
                name: "Units",
                newName: "Unit");

            migrationBuilder.RenameTable(
                name: "Lessons",
                newName: "Lesson");

            migrationBuilder.RenameTable(
                name: "LearningObjectives",
                newName: "LearningObjective");

            migrationBuilder.RenameIndex(
                name: "IX_Units_ProjectId",
                table: "Unit",
                newName: "IX_Unit_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_UnitId",
                table: "Lesson",
                newName: "IX_Lesson_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningObjectives_SchemaId",
                table: "LearningObjective",
                newName: "IX_LearningObjective_SchemaId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningObjectives_LessonId",
                table: "LearningObjective",
                newName: "IX_LearningObjective_LessonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Unit",
                table: "Unit",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lesson",
                table: "Lesson",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningObjective",
                table: "LearningObjective",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningObjective_Lesson_LessonId",
                table: "LearningObjective",
                column: "LessonId",
                principalTable: "Lesson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningObjective_Schemas_SchemaId",
                table: "LearningObjective",
                column: "SchemaId",
                principalTable: "Schemas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_Unit_UnitId",
                table: "Lesson",
                column: "UnitId",
                principalTable: "Unit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Unit_Projects_ProjectId",
                table: "Unit",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
