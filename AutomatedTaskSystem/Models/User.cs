using System.ComponentModel.DataAnnotations;

namespace AutomatedTaskSystem.Models
{
	public class User
	{
		public bool Archived { get; set; } = false;
		public int Id { get; set; }
		[MaxLength(6), MinLength(6)]
		public string Code { get; set; } = "";
		public bool OnBoard { get; set; } = false;
		public string Name { get; set; } = "";
		public List<Project> Projects { get; set; } = new List<Project> {};
		public List<Task> Tasks { get; set; } = new List<Task> {};
		public Team? Team { get; set; } = null;
		public int? TeamId { get; set; }
		public Group Group { get; set; } = new Group { };
		public int GroupId { get; set; }
		public Role Role { get; set; } = new Role { };
		public int RoleId { get; set; }
		public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken> { };
	}
}
