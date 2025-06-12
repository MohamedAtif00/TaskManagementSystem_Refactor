using AutomatedTaskSystem.Dtos.LeaveDtos;
using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Dtos.WorkFromHomeDtos
{
    public class GetSingleWorkFromHomeDto
    {
        public int Id { get; set; }
        public string Date { get; set; } // Formatted date string (e.g., "M/d/yyyy h:mm:ss tt")
        public string? NoteForManager { get; set; }
        public string Status { get; set; } // Current overall status of the request
        public UserDTO? User { get; set; } // Detailed user information
        public string? DateCreated { get; set; } // Date the request was created
        public List<GetOpinion> Opinions { get; set; } = new(); // List of opinions associated with this request
    }
}
