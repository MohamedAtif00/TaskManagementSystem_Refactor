using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class UpdateLeaveRequestDto
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string? Reason { get; set; }
        public LeaveRequestStatusEnum Status { get; set; }
    }
}
