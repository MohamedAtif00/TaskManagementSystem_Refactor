namespace AutomatedTaskSystem.Models
{
	public class Section
	{
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public User Head { get; set; } = new User { };
        public int HeadId { get; set; }
        public List<SectionGroup> SectionGroups { get; set; } = new List<SectionGroup> { };
        public bool Archived { get; set; } = false;
    }
}