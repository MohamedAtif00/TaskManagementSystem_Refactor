using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Enums.UserRole;

namespace AutomatedTaskSystem.Dtos.LeaveDtos
{
    public class GetOpinion
    {
        public int Id { get; set; }
        public int? LeaveRequestId { get; set; } // The leave request being commented on
        public int? PermissionId { get; set; } // The permission request being commented on
        public int? WorkFromHomeRequestId { get; set; } // The permission request being commented on
        public string Comment { get; set; } // The comment text
        public string DateCreated { get; set; }  // When the comment was made
        public bool IsApproved { get; set; } = false; // Whether the comment is approved or not
        public IDNameWithRole User { get; set; } // Navigation property to the user who made the comment

    }

    public class IDNameWithRole{
        public int Id { get; set; }
        public string Name { get; set; }
        public UserRoleEnum Role { get; set; }
    }

    
}
