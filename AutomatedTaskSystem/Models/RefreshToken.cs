using System.ComponentModel.DataAnnotations;

namespace AutomatedTaskSystem.Models
{
    public class RefreshToken
    {
        [Key]
        public string Token { get; set; } = "";
        public bool Used { get; set; } = false;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Expires { get; set; } = DateTime.UtcNow.AddMinutes(15);
        public User User { get; set; } = new User { };
        public int UserId { get; set; }
    }
}
