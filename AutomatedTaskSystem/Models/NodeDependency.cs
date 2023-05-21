namespace AutomatedTaskSystem.Models
{
	public class NodeDependency
	{
		public int RequiredId { get; set; }
		public int RequiresId { get; set; }
	}
}