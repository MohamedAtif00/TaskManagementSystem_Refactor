namespace AutomatedTaskSystem.Dtos.Report;

public class GetLessonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RunningLearningObjectives { get; set; }
    public int DoneLearningObjectives { get; set; }
    public List<GetLearningObjectiveDto> LearningObjectives { get; set; } =
        new List<GetLearningObjectiveDto> { };
}
