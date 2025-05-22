using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.PermissionDtos
{
    public class UpdatePermissionDto
    {
        public int Id { get; set; }
        public PermissionType Type { get; set; } 
        public string Reason { get; set; } = string.Empty;
    }

}
