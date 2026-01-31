using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;

namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class GetSingleLeaveRequestDto :BaseLeaveDto
    {
        public int Id { get; set; }
        public string NoteToManager { get; set; }
        public string DateCreated { get; set; }
        public int Duration { get; set; }
        public string Type { get; set; }
        public string Status { get; set; } = LeaveRequestStatusEnum.Pending.ToString();
        public Responses.UserDTO user { get; set; }
        public List<GetOpinion> Opinions { get; set; } = new();
    }
}
