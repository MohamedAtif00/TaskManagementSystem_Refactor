namespace AutomatedTaskSystem.Models
{
	public class Lesson
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public Unit Unit { get; set; } = new Unit { };
		public int UnitId { get; set; }
		public List<LearningObjective> LearningObjectives { get; set; } = new List<LearningObjective> { };
		public bool Archived { get; set; } = false;
	}
}