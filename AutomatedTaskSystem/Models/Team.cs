namespace AutomatedTaskSystem.Models
{
	public class Team
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public List<User> Users { get; set; } = new List<User> { };
		public User TeamLeader = new User { };
		public int TeamLeaderId;
		public bool Archived { get; set; } = false;
	}
}