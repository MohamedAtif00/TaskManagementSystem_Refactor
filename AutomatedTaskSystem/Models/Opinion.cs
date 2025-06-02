namespace AutomatedTaskSystem.Models
{
    public class Opinion
    {
        public int Id { get; set; }
        public int UserId { get; set; } // The user who gave the opinion
        public int? LeaveRequestId { get; set; } // The leave request being commented on
        public int? PermissionId { get; set; }
        public string? Comment { get; set; } // The comment text
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // When the comment was made
        public bool IsApproved { get; set; } = false; // Whether the comment is approved or not
        public virtual User User { get; set; } // Navigation property to the user who made the comment

        public virtual LeaveRequest? LeaveRequest { get; set; } // Navigation property to the leave request
        public virtual Permission? Permission { get; set; }

    }
}
