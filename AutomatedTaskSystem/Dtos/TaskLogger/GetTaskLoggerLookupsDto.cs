namespace AutomatedTaskSystem.Dtos.TaskLogger;

public class GetTaskLoggerLookupsDto
{
    public List<string> Members { get; set; } = new();
    public List<string> Subjects { get; set; } = new();
    public List<string> TaskNames { get; set; } = new();
    public List<string> Statuses { get; set; } = new();
}
