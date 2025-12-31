namespace AutomatedTaskSystem.Models
{
    public class Sprint
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsArchived { get; set; }
        public List<SprintLearningObjective> SprintLearningObjectives { get; set; } = new();
    }
}
