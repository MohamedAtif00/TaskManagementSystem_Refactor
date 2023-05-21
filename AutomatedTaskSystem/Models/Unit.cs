namespace AutomatedTaskSystem.Models
{
	public class Unit
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public Project Project { get; set; } = new Project { };
		public int ProjectId { get; set; }
		public List<Lesson> Lessons { get; set; } = new List<Lesson> { };
		public bool Archived { get; set; } = false;
	}
}