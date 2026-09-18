namespace TaskManagementSystem.Modules.Sprints.Domain;

public sealed class SprintLearningObjective
{
    private SprintLearningObjective()
    {
    }

    public int Id { get; internal set; }
    public int SprintId { get; internal set; }
    public int LearningObjectiveId { get; internal set; }

    internal static SprintLearningObjective Create(int sprintId, int learningObjectiveId) =>
        new() { SprintId = sprintId, LearningObjectiveId = learningObjectiveId };
}
