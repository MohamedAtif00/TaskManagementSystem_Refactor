using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class TaskOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BasicInfoDto Group { get; set; } = new BasicInfoDto { };
    public bool TeamLead { get; set; }
}
