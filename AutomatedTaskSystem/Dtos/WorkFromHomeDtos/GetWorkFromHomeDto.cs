using static AutomatedTaskSystem.DTO.Responses;

namespace AutomatedTaskSystem.Dtos.WorkFromHomeDtos
{
    public class GetWorkFromHomeDto
    {
        public int Id { get; set; }
        public string Date { get; set; } // Formatted date string (e.g., "yyyy-MM-dd")
        public string? NoteForManager { get; set; }
        public string Status { get; set; } // Current overall status of the request (e.g., "Pending", "Approved")
        public IDName? User { get; set; } // A simplified DTO for the user (Id and Name)
        public string? DateCreated { get; set; } // Date the request was created
        public string MyStatus { get; set; } // The current user's opinion status for this request (e.g., "Pending", "Approved", "Rejected")
    }
}
