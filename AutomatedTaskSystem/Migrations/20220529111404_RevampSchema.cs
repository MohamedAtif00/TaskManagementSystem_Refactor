using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class RevampSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjectives_LearningObjectivesTypes_TypeId",
                table: "LearningObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_Schemas_Groups_GroupId",
                table: "Schemas");

            migrationBuilder.DropForeignKey(
                name: "FK_Schemas_LearningObjectivesTypes_TypeId",
                table: "Schemas");

            migrationBuilder.DropTable(
                name: "LearningObjectivesTypes");

            migrationBuilder.DropIndex(
                name: "IX_Schemas_GroupId",
                table: "Schemas");

            migrationBuilder.DropIndex(
                name: "IX_Schemas_TypeId",
                table: "Schemas");

            migrationBuilder.DropIndex(
                name: "IX_LearningObjectives_TypeId",
                table: "LearningObjectives");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Schemas");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Schemas");

            migrationBuilder.DropColumn(
                name: "IsStart",
                table: "Schemas");

            migrationBuilder.DropColumn(
                name: "Next",
                table: "Schemas");

            migrationBuilder.DropColumn(
                name: "Previous",
                table: "Schemas");

            migrationBuilder.DropColumn(
                name: "Serial",
                table: "Schemas");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Schemas");

            migrationBuilder.AddColumn<int>(
                name: "SchemaId",
                table: "LearningObjectives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SchemaSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsStart = table.Column<bool>(type: "bit", nullable: false),
                    Serial = table.Column<int>(type: "int", nullable: false),
                    Next = table.Column<int>(type: "int", nullable: true),
                    Previous = table.Column<int>(type: "int", nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    SchemaId = table.Column<int>(type: "int", nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaSteps_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SchemaSteps_Schemas_SchemaId",
                        column: x => x.SchemaId,
                        principalTable: "Schemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningObjectives_SchemaId",
                table: "LearningObjectives",
                column: "SchemaId");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaSteps_GroupId",
                table: "SchemaSteps",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaSteps_SchemaId",
                table: "SchemaSteps",
                column: "SchemaId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningObjectives_Schemas_SchemaId",
                table: "LearningObjectives",
                column: "SchemaId",
                principalTable: "Schemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningObjectives_Schemas_SchemaId",
                table: "LearningObjectives");

            migrationBuilder.DropTable(
                name: "SchemaSteps");

            migrationBuilder.DropIndex(
                name: "IX_LearningObjectives_SchemaId",
                table: "LearningObjectives");

            migrationBuilder.DropColumn(
                name: "SchemaId",
                table: "LearningObjectives");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Schemas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Schemas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsStart",
                table: "Schemas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Next",
                table: "Schemas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Previous",
                table: "Schemas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Serial",
                table: "Schemas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Schemas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LearningObjectivesTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningObjectivesTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schemas_GroupId",
                table: "Schemas",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Schemas_TypeId",
                table: "Schemas",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningObjectives_TypeId",
                table: "LearningObjectives",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningObjectives_LearningObjectivesTypes_TypeId",
                table: "LearningObjectives",
                column: "TypeId",
                principalTable: "LearningObjectivesTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schemas_Groups_GroupId",
                table: "Schemas",
                column: "GroupId",
                principalTable: "Groups",
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
    }
}
