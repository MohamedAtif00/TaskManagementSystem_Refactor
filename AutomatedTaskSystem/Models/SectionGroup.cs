namespace AutomatedTaskSystem.Models
{
    public class SectionGroup
    {
        public int Id { get; set; }
        public Section Section { get; set; }
        public int SectionId { get; set; }
        public Group Group { get; set; }
        public int GroupId { get; set; }
    }
}
