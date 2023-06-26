namespace AutomatedTaskSystem.Dtos.Report;

public class GetUnitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RunningLearningObjectives { get; set; }
    public int DoneLearningObjectives { get; set; }
    public List<GetLessonDto> Lessons { get; set; } = new List<GetLessonDto> { };
}
