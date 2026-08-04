using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectCopiedFromSubjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CopiedFromSubjectId",
                table: "Subjects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_CopiedFromSubjectId",
                table: "Subjects",
                column: "CopiedFromSubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Subjects_CopiedFromSubjectId",
                table: "Subjects",
                column: "CopiedFromSubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Subjects_CopiedFromSubjectId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_CopiedFromSubjectId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "CopiedFromSubjectId",
                table: "Subjects");
        }
    }
}
