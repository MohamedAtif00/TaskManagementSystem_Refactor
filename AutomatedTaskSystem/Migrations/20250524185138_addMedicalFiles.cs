using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    public partial class addMedicalFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MedicalCertificateFileName",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalCertificatePath",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicalCertificateFileName",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "MedicalCertificatePath",
                table: "LeaveRequests");
        }
    }
}
