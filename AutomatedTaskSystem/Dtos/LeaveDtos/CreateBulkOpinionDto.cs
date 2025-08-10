using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class CreateBulkOpinionDto
    {
        public int[] Ids { get; set; } = Array.Empty<int>();
        public LeaveRequestStatusEnum Status { get; set; } 
    }
}
