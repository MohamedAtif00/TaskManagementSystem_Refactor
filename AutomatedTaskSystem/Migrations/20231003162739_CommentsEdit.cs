using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class CommentsEdit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChildId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Comments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));


            migrationBuilder.Sql(
                @"
				UPDATE Comments
				SET CreatedAt = TimeStamp;"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ChildId",
                table: "Comments",
                column: "ChildId",
                unique: true,
                filter: "[ChildId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ChildId",
                table: "Comments",
                column: "ChildId",
                principalTable: "Comments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ChildId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ChildId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ChildId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Comments");
        }
    }
}
