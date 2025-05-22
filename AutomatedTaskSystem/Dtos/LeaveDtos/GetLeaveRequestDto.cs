using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class GetLeaveRequestDto
    {
        public int Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string? Reason { get; set; }
        public string DateCreated { get; set; }
        public int Duration { get; set; }
        public string Type {get;set;}
        public string Status { get; set; } = LeaveRequestStatusEnum.Pending.ToString();
        public Responses.IDName user { get;set; }
    }
}
