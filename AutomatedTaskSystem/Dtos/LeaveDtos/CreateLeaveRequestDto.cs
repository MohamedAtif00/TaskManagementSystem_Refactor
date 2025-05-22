using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class CreateLeaveRequestDto
    {
        public int UserId { get; set; }
        public LeaveRequestType type { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string? Reason { get; set; }
        public string? NoteForManager { get; set; }
    }
}
