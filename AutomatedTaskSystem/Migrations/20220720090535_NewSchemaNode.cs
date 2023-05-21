using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class NewSchemaNode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeNode_Nodes_NextNodesId",
                table: "NodeNode");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeNode_Nodes_PreviousNodesId",
                table: "NodeNode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NodeNode",
                table: "NodeNode");

            migrationBuilder.RenameTable(
                name: "NodeNode",
                newName: "NodeDependencies");

            migrationBuilder.RenameColumn(
                name: "PreviousNodesId",
                table: "NodeDependencies",
                newName: "PreviousId");

            migrationBuilder.RenameColumn(
                name: "NextNodesId",
                table: "NodeDependencies",
                newName: "NextId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeNode_PreviousNodesId",
                table: "NodeDependencies",
                newName: "IX_NodeDependencies_PreviousId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NodeDependencies",
                table: "NodeDependencies",
                columns: new[] { "NextId", "PreviousId" });

            migrationBuilder.CreateTable(
                name: "NodeSequences",
                columns: table => new
                {
                    RequiredId = table.Column<int>(type: "int", nullable: false),
                    RequiresId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeSequences", x => new { x.RequiredId, x.RequiresId });
                    table.ForeignKey(
                        name: "FK_NodeSequences_Nodes_RequiredId",
                        column: x => x.RequiredId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodeSequences_Nodes_RequiresId",
                        column: x => x.RequiresId,
                        principalTable: "Nodes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodeSequences_RequiresId",
                table: "NodeSequences",
                column: "RequiresId");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeDependencies_Nodes_NextId",
                table: "NodeDependencies",
                column: "NextId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeDependencies_Nodes_PreviousId",
                table: "NodeDependencies",
                column: "PreviousId",
                principalTable: "Nodes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeDependencies_Nodes_NextId",
                table: "NodeDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeDependencies_Nodes_PreviousId",
                table: "NodeDependencies");

            migrationBuilder.DropTable(
                name: "NodeSequences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NodeDependencies",
                table: "NodeDependencies");

            migrationBuilder.RenameTable(
                name: "NodeDependencies",
                newName: "NodeNode");

            migrationBuilder.RenameColumn(
                name: "PreviousId",
                table: "NodeNode",
                newName: "PreviousNodesId");

            migrationBuilder.RenameColumn(
                name: "NextId",
                table: "NodeNode",
                newName: "NextNodesId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeDependencies_PreviousId",
                table: "NodeNode",
                newName: "IX_NodeNode_PreviousNodesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NodeNode",
                table: "NodeNode",
                columns: new[] { "NextNodesId", "PreviousNodesId" });

            migrationBuilder.AddForeignKey(
                name: "FK_NodeNode_Nodes_NextNodesId",
                table: "NodeNode",
                column: "NextNodesId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeNode_Nodes_PreviousNodesId",
                table: "NodeNode",
                column: "PreviousNodesId",
                principalTable: "Nodes",
                principalColumn: "Id");
        }
    }
}
