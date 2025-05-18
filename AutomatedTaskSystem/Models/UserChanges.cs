using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AutomatedTaskSystem.Models
{
    public class UserChanges
    {
        public int Id { get; set; }

        public int UserId { get; set; }          // The user who was modified

        public int ChangedByUserId { get; set; } // Who made the change

        public string ChangedByUserName { get; set; }

        public string Action { get; set; }       // "Created", "Updated", "Deleted"

        public string Changes { get; set; }      // Detailed change log (e.g., "Name: Old → New")

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; }

        public virtual User ChangedByUser { get; set; }
    }
}
