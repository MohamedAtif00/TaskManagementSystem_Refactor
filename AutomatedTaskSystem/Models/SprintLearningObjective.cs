namespace AutomatedTaskSystem.Models
{
    public class SprintLearningObjective
    {
        public int Id { get; set; }
        public int SprintId { get; set; }
        public Sprint? Sprint { get; set; }
        public int LearningObjectiveId { get; set; }
        public LearningObjective? LearningObjective { get; set; }
    }
}
