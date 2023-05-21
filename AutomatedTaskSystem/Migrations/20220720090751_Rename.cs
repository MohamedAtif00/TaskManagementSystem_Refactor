using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class Rename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeDependencies_Nodes_NextId",
                table: "NodeDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeDependencies_Nodes_PreviousId",
                table: "NodeDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeSequences_Nodes_RequiredId",
                table: "NodeSequences");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeSequences_Nodes_RequiresId",
                table: "NodeSequences");

            migrationBuilder.RenameColumn(
                name: "RequiresId",
                table: "NodeSequences",
                newName: "PreviousId");

            migrationBuilder.RenameColumn(
                name: "RequiredId",
                table: "NodeSequences",
                newName: "NextId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeSequences_RequiresId",
                table: "NodeSequences",
                newName: "IX_NodeSequences_PreviousId");

            migrationBuilder.RenameColumn(
                name: "PreviousId",
                table: "NodeDependencies",
                newName: "RequiresId");

            migrationBuilder.RenameColumn(
                name: "NextId",
                table: "NodeDependencies",
                newName: "RequiredId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeDependencies_PreviousId",
                table: "NodeDependencies",
                newName: "IX_NodeDependencies_RequiresId");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeDependencies_Nodes_RequiredId",
                table: "NodeDependencies",
                column: "RequiredId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeDependencies_Nodes_RequiresId",
                table: "NodeDependencies",
                column: "RequiresId",
                principalTable: "Nodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeSequences_Nodes_NextId",
                table: "NodeSequences",
                column: "NextId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeSequences_Nodes_PreviousId",
                table: "NodeSequences",
                column: "PreviousId",
                principalTable: "Nodes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeDependencies_Nodes_RequiredId",
                table: "NodeDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeDependencies_Nodes_RequiresId",
                table: "NodeDependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeSequences_Nodes_NextId",
                table: "NodeSequences");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeSequences_Nodes_PreviousId",
                table: "NodeSequences");

            migrationBuilder.RenameColumn(
                name: "PreviousId",
                table: "NodeSequences",
                newName: "RequiresId");

            migrationBuilder.RenameColumn(
                name: "NextId",
                table: "NodeSequences",
                newName: "RequiredId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeSequences_PreviousId",
                table: "NodeSequences",
                newName: "IX_NodeSequences_RequiresId");

            migrationBuilder.RenameColumn(
                name: "RequiresId",
                table: "NodeDependencies",
                newName: "PreviousId");

            migrationBuilder.RenameColumn(
                name: "RequiredId",
                table: "NodeDependencies",
                newName: "NextId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeDependencies_RequiresId",
                table: "NodeDependencies",
                newName: "IX_NodeDependencies_PreviousId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_NodeSequences_Nodes_RequiredId",
                table: "NodeSequences",
                column: "RequiredId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeSequences_Nodes_RequiresId",
                table: "NodeSequences",
                column: "RequiresId",
                principalTable: "Nodes",
                principalColumn: "Id");
        }
    }
}
