using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class ReworkSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUserProject_AspNetUsers_UsersId",
                table: "AppUserProject");

            migrationBuilder.DropTable(
                name: "LearningObjectives");

            migrationBuilder.DropTable(
                name: "SchemaSteps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUserProject",
                table: "AppUserProject");

            migrationBuilder.DropIndex(
                name: "IX_AppUserProject_UsersId",
                table: "AppUserProject");

            migrationBuilder.RenameColumn(
                name: "UsersId",
                table: "AppUserProject",
                newName: "AssigneesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUserProject",
                table: "AppUserProject",
                columns: new[] { "AssigneesId", "ProjectsId" });

            migrationBuilder.CreateTable(
                name: "Phases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchemaId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Phases_Schemas_SchemaId",
                        column: x => x.SchemaId,
                        principalTable: "Schemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhaseId = table.Column<int>(type: "int", nullable: false),
                    NextPhaseId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blocks_Phases_NextPhaseId",
                        column: x => x.NextPhaseId,
                        principalTable: "Phases",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Blocks_Phases_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "Phases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextStepId = table.Column<int>(type: "int", nullable: true),
                    PreviousStepId = table.Column<int>(type: "int", nullable: true),
                    BlockId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Steps_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Steps_Steps_NextStepId",
                        column: x => x.NextStepId,
                        principalTable: "Steps",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserProject_ProjectsId",
                table: "AppUserProject",
                column: "ProjectsId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_NextPhaseId",
                table: "Blocks",
                column: "NextPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_PhaseId",
                table: "Blocks",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Phases_SchemaId",
                table: "Phases",
                column: "SchemaId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_BlockId",
                table: "Steps",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_NextStepId",
                table: "Steps",
                column: "NextStepId",
                unique: true,
                filter: "[NextStepId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUserProject_AspNetUsers_AssigneesId",
                table: "AppUserProject",
                column: "AssigneesId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUserProject_AspNetUsers_AssigneesId",
                table: "AppUserProject");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropTable(
                name: "Phases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUserProject",
                table: "AppUserProject");

            migrationBuilder.DropIndex(
                name: "IX_AppUserProject_ProjectsId",
                table: "AppUserProject");

            migrationBuilder.RenameColumn(
                name: "AssigneesId",
                table: "AppUserProject",
                newName: "UsersId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUserProject",
                table: "AppUserProject",
                columns: new[] { "ProjectsId", "UsersId" });

            migrationBuilder.CreateTable(
                name: "LearningObjectives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    SchemaId = table.Column<int>(type: "int", nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lesson = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningObjectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningObjectives_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningObjectives_Schemas_SchemaId",
                        column: x => x.SchemaId,
                        principalTable: "Schemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchemaSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    SchemaId = table.Column<int>(type: "int", nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    ForTeamLeader = table.Column<bool>(type: "bit", nullable: false),
                    IsStart = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Next = table.Column<int>(type: "int", nullable: true),
                    Requires = table.Column<int>(type: "int", nullable: true),
                    Serial = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_AppUserProject_UsersId",
                table: "AppUserProject",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningObjectives_ProjectId",
                table: "LearningObjectives",
                column: "ProjectId");

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
                name: "FK_AppUserProject_AspNetUsers_UsersId",
                table: "AppUserProject",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
