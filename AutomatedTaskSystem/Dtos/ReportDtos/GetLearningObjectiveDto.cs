namespace AutomatedTaskSystem.Dtos.Report;

public class GetLearningObjectiveDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? Started { get; set; } = null;
    public DateTime? Done { get; set; } = null;
}
