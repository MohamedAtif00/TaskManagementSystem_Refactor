using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class RemoveRedundantColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_TaskBank_TaskBankId",
                table: "Steps");

            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Types_TypeId",
                table: "Steps");

            migrationBuilder.DropTable(
                name: "GroupStep");

            migrationBuilder.DropIndex(
                name: "IX_Steps_TypeId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "Reviewable",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "TL",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Steps");

            migrationBuilder.AlterColumn<int>(
                name: "TaskBankId",
                table: "Steps",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Steps",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2023, 3, 8, 12, 46, 47, 606, DateTimeKind.Local).AddTicks(7870),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2023, 2, 12, 15, 11, 0, 544, DateTimeKind.Local).AddTicks(7380));

            migrationBuilder.CreateIndex(
                name: "IX_Steps_GroupId",
                table: "Steps",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Groups_GroupId",
                table: "Steps",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_TaskBank_TaskBankId",
                table: "Steps",
                column: "TaskBankId",
                principalTable: "TaskBank",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Groups_GroupId",
                table: "Steps");

            migrationBuilder.DropForeignKey(
                name: "FK_Steps_TaskBank_TaskBankId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Steps_GroupId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Steps");

            migrationBuilder.AlterColumn<int>(
                name: "TaskBankId",
                table: "Steps",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Steps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Reviewable",
                table: "Steps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TL",
                table: "Steps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Steps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2023, 2, 12, 15, 11, 0, 544, DateTimeKind.Local).AddTicks(7380),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2023, 3, 8, 12, 46, 47, 606, DateTimeKind.Local).AddTicks(7870));

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

            migrationBuilder.CreateIndex(
                name: "IX_Steps_TypeId",
                table: "Steps",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupStep_StepsId",
                table: "GroupStep",
                column: "StepsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_TaskBank_TaskBankId",
                table: "Steps",
                column: "TaskBankId",
                principalTable: "TaskBank",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Types_TypeId",
                table: "Steps",
                column: "TypeId",
                principalTable: "Types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
