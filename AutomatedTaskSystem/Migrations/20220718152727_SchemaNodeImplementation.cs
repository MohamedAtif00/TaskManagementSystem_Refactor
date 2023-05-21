using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class SchemaNodeImplementation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Blocks_BlockId",
                table: "Steps");

            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Groups_GroupId",
                table: "Steps");

            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropTable(
                name: "Phases");

            migrationBuilder.DropIndex(
                name: "IX_Steps_GroupId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Steps");

            migrationBuilder.RenameColumn(
                name: "BlockId",
                table: "Steps",
                newName: "NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Steps_BlockId",
                table: "Steps",
                newName: "IX_Steps_NodeId");

            migrationBuilder.CreateTable(
                name: "GroupStep",
                columns: table => new
                {
                    GroupsId = table.Column<int>(type: "int", nullable: false),
                    StepsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupStep", x => new { x.GroupsId, x.StepsId });
                    table.ForeignKey(
                        name: "FK_GroupStep_Groups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupStep_Steps_StepsId",
                        column: x => x.StepsId,
                        principalTable: "Steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchemaId = table.Column<int>(type: "int", nullable: false),
                    isStart = table.Column<bool>(type: "bit", nullable: false),
                    isEnd = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nodes_Schemas_SchemaId",
                        column: x => x.SchemaId,
                        principalTable: "Schemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodeNode",
                columns: table => new
                {
                    NextNodesId = table.Column<int>(type: "int", nullable: false),
                    PreviousNodesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeNode", x => new { x.NextNodesId, x.PreviousNodesId });
                    table.ForeignKey(
                        name: "FK_NodeNode_Nodes_NextNodesId",
                        column: x => x.NextNodesId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodeNode_Nodes_PreviousNodesId",
                        column: x => x.PreviousNodesId,
                        principalTable: "Nodes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupStep_StepsId",
                table: "GroupStep",
                column: "StepsId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeNode_PreviousNodesId",
                table: "NodeNode",
                column: "PreviousNodesId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_SchemaId",
                table: "Nodes",
                column: "SchemaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Nodes_NodeId",
                table: "Steps",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Nodes_NodeId",
                table: "Steps");

            migrationBuilder.DropTable(
                name: "GroupStep");

            migrationBuilder.DropTable(
                name: "NodeNode");

            migrationBuilder.DropTable(
                name: "Nodes");

            migrationBuilder.RenameColumn(
                name: "NodeId",
                table: "Steps",
                newName: "BlockId");

            migrationBuilder.RenameIndex(
                name: "IX_Steps_NodeId",
                table: "Steps",
                newName: "IX_Steps_BlockId");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Steps",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Phases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemaId = table.Column<int>(type: "int", nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
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
                    NextPhaseId = table.Column<int>(type: "int", nullable: true),
                    PhaseId = table.Column<int>(type: "int", nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Steps_GroupId",
                table: "Steps",
                column: "GroupId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Blocks_BlockId",
                table: "Steps",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Groups_GroupId",
                table: "Steps",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
