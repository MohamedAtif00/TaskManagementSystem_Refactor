namespace AutomatedTaskSystem.Models
{
	public class Section
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public User Head { get; set; } = new User { };
		public int HeadId { get; set; }
		public List<Group> Groups { get; set; } = new List<Group> { };
		public bool Archived { get; set; } = false;
	}
}