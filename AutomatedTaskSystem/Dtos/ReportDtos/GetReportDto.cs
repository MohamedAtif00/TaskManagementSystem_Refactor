namespace AutomatedTaskSystem.Dtos.Report;

public class GetReportDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public int IdleLearningObjectives { get; set; }
    public int RunningLearningObjectives { get; set; }
    public int DoneLearningObjectives { get; set; }
}
