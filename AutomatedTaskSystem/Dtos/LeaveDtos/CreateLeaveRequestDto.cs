using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class CreateLeaveRequestDto : BaseLeaveDto
    {
        public int UserId { get; set; }
        public LeaveRequestType type { get; set; }
        public string? NoteForManager { get; set; } = null;
        public IFormFile? MedicalCertificate { get; set; }
    }
}
