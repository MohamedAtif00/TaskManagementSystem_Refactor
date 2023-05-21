using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
	public partial class NameForUserAndArchives : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "Archived",
				table: "Titles",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "Archived",
				table: "Teams",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "Archived",
				table: "TaskStatus",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "Archived",
				table: "Tasks",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "Archived",
				table: "Schemas",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "Archived",
				table: "Projects",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "Archived",
				table: "LearningObjectives",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "Archived",
				table: "LeaningObjectivesTypes",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "Archived",
				table: "Groups",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "Archived",
				table: "AspNetUsers",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<string>(
				name: "FirstName",
				table: "AspNetUsers",
				type: "nvarchar(max)",
				nullable: false,
				defaultValue: "");

			migrationBuilder.AddColumn<int>(
				name: "GroupId",
				table: "AspNetUsers",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<string>(
				name: "LastName",
				table: "AspNetUsers",
				type: "nvarchar(max)",
				nullable: false,
				defaultValue: "");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUsers_GroupId",
				table: "AspNetUsers",
				column: "GroupId");

			migrationBuilder.AddForeignKey(
				name: "FK_AspNetUsers_Groups_GroupId",
				table: "AspNetUsers",
				column: "GroupId",
				principalTable: "Groups",
				principalColumn: "Id",
				onDelete: ReferentialAction.SetDefault);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_AspNetUsers_Groups_GroupId",
				table: "AspNetUsers");

			migrationBuilder.DropIndex(
				name: "IX_AspNetUsers_GroupId",
				table: "AspNetUsers");

			migrationBuilder.DropColumn(
				name: "Archived",
				table: "Titles");

			migrationBuilder.DropColumn(
				name: "Archived",
				table: "Teams");

			migrationBuilder.DropColumn(
				name: "Archived",
				table: "TaskStatus");

			migrationBuilder.DropColumn(
				name: "Archived",
				table: "Tasks");

			migrationBuilder.DropColumn(
				name: "Archived",
				table: "Schemas");

			migrationBuilder.DropColumn(
				name: "Archived",
				table: "Projects");

			migrationBuilder.DropColumn(
				name: "Archived",
				table: "LearningObjectives");

			migrationBuilder.DropColumn(
				name: "Archived",
				table: "LeaningObjectivesTypes");

			migrationBuilder.DropColumn(
				name: "Archived",
				table: "Groups");

			migrationBuilder.DropColumn(
				name: "Archived",
				table: "AspNetUsers");

			migrationBuilder.DropColumn(
				name: "FirstName",
				table: "AspNetUsers");

			migrationBuilder.DropColumn(
				name: "GroupId",
				table: "AspNetUsers");

			migrationBuilder.DropColumn(
				name: "LastName",
				table: "AspNetUsers");
		}
	}
}
