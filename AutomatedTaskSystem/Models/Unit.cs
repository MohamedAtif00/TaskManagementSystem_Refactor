namespace AutomatedTaskSystem.Models
{
	public class Unit
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public Subject Subject { get; set; } = new Subject { };
		public int SubjectId { get; set; }
		public List<Lesson> Lessons { get; set; } = new List<Lesson> { };
		public bool Archived { get; set; } = false;
	}
}