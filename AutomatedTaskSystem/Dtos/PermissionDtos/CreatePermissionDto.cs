using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.PermissionDtos
{
    public class CreatePermissionDto
    {
        public PermissionType Type { get; set; } = PermissionType.Morning;  
        public int UserId { get; set; }
        public string Reason { get; set; } = "";
        public string PermissionDate { get; set; } = DateTime.UtcNow.ToString();

    }
}
