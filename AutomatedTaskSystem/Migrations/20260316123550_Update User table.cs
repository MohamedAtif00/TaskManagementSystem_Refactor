using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromNextBalanceDaysUsed",
                table: "LeaveRequests");

            migrationBuilder.AddColumn<int>(
                name: "FromNextBalanceDaysUsed",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OldAnnualBalance",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromNextBalanceDaysUsed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OldAnnualBalance",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "FromNextBalanceDaysUsed",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
