namespace AutomatedTaskSystem.Models
{
	public class Group
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string ColorCode { get; set; } = "";
		public Section? Section { get; set; } = null;
		public int? SectionId { get; set; } = null;
		public bool Archived { get; set; } = false;
		public List<User> Users { get; set; } = new List<User> { };
	}
}
