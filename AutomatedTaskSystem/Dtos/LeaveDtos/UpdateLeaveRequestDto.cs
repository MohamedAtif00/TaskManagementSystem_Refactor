using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class UpdateLeaveRequestDto : BaseLeaveDto
    {
        public LeaveRequestStatusEnum Status { get; set; }
    }
}
