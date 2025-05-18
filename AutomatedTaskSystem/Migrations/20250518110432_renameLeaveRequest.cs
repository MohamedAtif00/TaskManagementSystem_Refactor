using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class renameLeaveRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
             name: "Vacations",
             newName: "LeaveRequests");  // Change to your desired table name

            migrationBuilder.AddColumn<DateTime>(
               name: "DateCreated",
               table: "LeaveRequest",  // Replace with your actual table name
               type: "datetime2",
               nullable: true,
               defaultValueSql: "GETDATE()"); // Or defaultValue: DateTime.UtcNow
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
               name: "LeaveRequests",
               newName: "Vacations");

            migrationBuilder.DropColumn(
            name: "DateCreated",
            table: "LeaveRequest");
        }
    }
}
