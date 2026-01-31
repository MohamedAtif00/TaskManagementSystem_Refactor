using System.ComponentModel.DataAnnotations;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class GetLeaveRequestDto : BaseLeaveDto
    {
        public int Id { get; set; }
        public string DateCreated { get; set; }
        public int Duration { get; set; }
        public string Type {get;set;}
        public string Status { get; set; } = LeaveRequestStatusEnum.Pending.ToString();
        public string? MyStatus { get; set; }
        public Responses.IDName user { get;set; }
    }
}
