using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Models.Enums.TaskStatus;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class GetTaskChipDto {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BasicInfoDto Group { get; set; } = new BasicInfoDto {};
    public BasicInfoDto? User { get; set; }
    public TaskStatusEnum Status { get; set; }
    public int RollbackCounts { get; set; }
    public bool Paused { get; set; }
    public bool IsRollback { get; set; }
}
