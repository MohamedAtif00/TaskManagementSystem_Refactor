namespace AutomatedTaskSystem.Models
{
	public class TaskBank
	{
		public bool Active { get; set; } = true;
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public bool TL { get; set; } = false;
		public Type Type { get; set; } = new Type { };
		public int TypeId { get; set; }
		public Group Group { get; set; } = new Group { };
		public int GroupId { get; set; }
        public int Duration { get; set; }
        public List<Step> Steps { get; set; } = new List<Step> {};
	}
}
