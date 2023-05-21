namespace AutomatedTaskSystem.Models
{
	public class Project
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string Description { get; set; } = "";
		public List<Unit> Units { get; set; } = new List<Unit> { };
		public List<User> Users { get; set; } = new List<User> { };
		public bool Archived { get; set; } = false;
	}
}