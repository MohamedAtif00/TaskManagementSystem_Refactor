using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.PermissionDtos
{
    public class CreatePermissionDto
    {
        public PermissionType Type { get; set; }
        public int UserId { get; set; }
        public string Reason { get; set; } = "";
        public string From { get; set; }
        public string To { get; set; }
        public string Date { get; set; } = DateTime.UtcNow.ToString();

    }
}
